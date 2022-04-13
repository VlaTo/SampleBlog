using Microsoft.Extensions.Logging;
using SampleBlog.IdentityServer.Core.Events;
using SampleBlog.IdentityServer.Services;
using SampleBlog.IdentityServer.Storage.Models;
using SampleBlog.IdentityServer.Storage.Stores;
using SampleBlog.IdentityServer.Validation;
using SampleBlog.IdentityServer.Validation.Contexts;

namespace SampleBlog.IdentityServer.Stores;

public class ValidatingClientStore<T> : IClientStore
    where T : IClientStore
{
    private readonly IClientStore store;
    private readonly IClientConfigurationValidator validator;
    private readonly IEventService events;
    private readonly ILogger<ValidatingClientStore<T>> logger;
    private readonly string _validatorType;

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidatingClientStore{T}" /> class.
    /// </summary>
    /// <param name="store">The inner.</param>
    /// <param name="validator">The validator.</param>
    /// <param name="events">The events.</param>
    /// <param name="logger">The logger.</param>
    public ValidatingClientStore(
        T store,
        IClientConfigurationValidator validator,
        IEventService events,
        ILogger<ValidatingClientStore<T>> logger)
    {
        this.store = store;
        this.validator = validator;
        this.events = events;
        this.logger = logger;

        _validatorType = validator.GetType().FullName;
    }

    /// <summary>
    /// Finds a client by id (and runs the validation logic)
    /// </summary>
    /// <param name="clientId">The client id</param>
    /// <returns>
    /// The client or an InvalidOperationException
    /// </returns>
    public async Task<Client?> FindClientByIdAsync(string clientId)
    {
        using var activity = Tracing.StoreActivitySource.StartActivity("ValidatingClientStore.FindClientById");

        var client = await store.FindClientByIdAsync(clientId);

        if (null != client)
        {
            logger.LogTrace("Calling into client configuration validator: {validatorType}", _validatorType);

            var context = new ClientConfigurationValidationContext(client);
            
            await validator.ValidateAsync(context);

            if (context.IsValid)
            {
                logger.LogDebug("client configuration validation for client {clientId} succeeded.", client.ClientId);
                return client;
            }

            logger.LogError("Invalid client configuration for client {clientId}: {errorMessage}", client.ClientId, context.ErrorMessage);
            
            await events.RaiseAsync(new InvalidClientConfigurationEvent(client, context.ErrorMessage));
        }

        return null;
    }
}