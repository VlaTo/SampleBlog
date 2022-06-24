using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SampleBlog.IdentityServer.Core;
using SampleBlog.IdentityServer.Extensions;
using SampleBlog.IdentityServer.Services;
using SampleBlog.IdentityServer.Storage.Stores;
using SampleBlog.IdentityServer.Validation.Results;

namespace SampleBlog.IdentityServer.Validation;

/// <summary>
/// Validates a client secret using the registered secret validators and parsers
/// </summary>
public class ClientSecretValidator : IClientSecretValidator
{
    private readonly IClientStore clients;
    private readonly ISecretsListParser parser;
    private readonly ISecretsListValidator validator;
    private readonly IEventService events;
    private readonly ILogger logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ClientSecretValidator"/> class.
    /// </summary>
    /// <param name="clients">The clients.</param>
    /// <param name="parser">The parser.</param>
    /// <param name="validator">The validator.</param>
    /// <param name="events">The events.</param>
    /// <param name="logger">The logger.</param>
    public ClientSecretValidator(
        IClientStore clients,
        ISecretsListParser parser,
        ISecretsListValidator validator,
        IEventService events,
        ILogger<ClientSecretValidator> logger)
    {
        this.clients = clients;
        this.parser = parser;
        this.validator = validator;
        this.events = events;
        this.logger = logger;
    }

    /// <summary>
    /// Validates the current request.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <returns></returns>
    public async Task<ClientSecretValidationResult> ValidateAsync(HttpContext context)
    {
        using var activity = Tracing.ActivitySource.StartActivity("ClientSecretValidator.Validate");

        logger.LogDebug("Start client validation");

        var fail = new ClientSecretValidationResult
        {
            IsError = true
        };

        var parsedSecret = await parser.ParseAsync(context);

        if (null == parsedSecret)
        {
            await RaiseFailureEventAsync("unknown", "No client id found");

            logger.LogError("No client identifier found");

            return fail;
        }

        // load client
        var client = await clients.FindEnabledClientByIdAsync(parsedSecret.Id);

        if (null == client)
        {
            await RaiseFailureEventAsync(parsedSecret.Id, "Unknown client");

            logger.LogError("No client with id '{clientId}' found. aborting", parsedSecret.Id);

            return fail;
        }

        SecretValidationResult? secretValidationResult = null;

        if (!client.RequireClientSecret || client.IsImplicitOnly())
        {
            logger.LogDebug("Public Client - skipping secret validation success");
        }
        else
        {
            secretValidationResult = await validator.ValidateAsync(client.ClientSecrets, parsedSecret);

            if (false == secretValidationResult.Success)
            {
                await RaiseFailureEventAsync(client.ClientId, "Invalid client secret");
                
                logger.LogError("Client secret validation failed for client: {clientId}.", client.ClientId);

                return fail;
            }
        }

        logger.LogDebug("Client validation success");

        var success = new ClientSecretValidationResult
        {
            IsError = false,
            Client = client,
            Secret = parsedSecret,
            Confirmation = secretValidationResult?.Confirmation
        };

        await RaiseSuccessEventAsync(client.ClientId, parsedSecret.Type);

        return success;
    }

    private Task RaiseSuccessEventAsync(string clientId, string authMethod)
    {
        return events.RaiseAsync(new ClientAuthenticationSuccessEvent(clientId, authMethod));
    }

    private Task RaiseFailureEventAsync(string? clientId, string message)
    {
        return events.RaiseAsync(new ClientAuthenticationFailureEvent(clientId, message));
    }
}