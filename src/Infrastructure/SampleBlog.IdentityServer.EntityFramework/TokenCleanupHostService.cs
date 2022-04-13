using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SampleBlog.IdentityServer.EntityFramework.Storage;
using SampleBlog.IdentityServer.EntityFramework.Storage.Options;

namespace SampleBlog.IdentityServer.EntityFramework;

/// <summary>
/// Helper to cleanup expired persisted grants.
/// </summary>
public class TokenCleanupHostService : IHostedService
{
    private readonly IServiceProvider serviceProvider;
    private readonly OperationalStoreOptions options;
    private readonly ILogger<TokenCleanupHostService> logger;
    private CancellationTokenSource? source;
    private readonly TimeSpan cleanupInterval;


    /// <summary>
    /// Constructor for TokenCleanupHost.
    /// </summary>
    /// <param name="serviceProvider"></param>
    /// <param name="options"></param>
    /// <param name="logger"></param>
    public TokenCleanupHostService(
        IServiceProvider serviceProvider,
        OperationalStoreOptions options,
        ILogger<TokenCleanupHostService> logger)
    {
        cleanupInterval = TimeSpan.FromSeconds(options.TokenCleanupInterval);

        this.serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        this.options = options ?? throw new ArgumentNullException(nameof(options));
        this.logger = logger;
    }

    /// <summary>
    /// Starts the token cleanup polling.
    /// </summary>
    public Task StartAsync(CancellationToken cancellationToken)
    {
        if (options.EnableTokenCleanup)
        {
            if (null != source)
            {
                throw new InvalidOperationException("Already started. Call Stop first.");
            }

            logger.LogDebug("Starting grant removal");

            source = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            Task.Factory.StartNew(() => StartInternalAsync(source.Token), cancellationToken);
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Stops the token cleanup polling.
    /// </summary>
    public Task StopAsync(CancellationToken cancellationToken)
    {
        if (options.EnableTokenCleanup)
        {
            if (null == source)
            {
                throw new InvalidOperationException("Not started. Call Start first.");
            }

            logger.LogDebug("Stopping grant removal");

            source.Cancel();
            source = null;
        }

        return Task.CompletedTask;
    }

    private async Task StartInternalAsync(CancellationToken cancellationToken)
    {
        while (true)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                logger.LogDebug("CancellationRequested. Exiting.");
                break;
            }

            try
            {
                await Task.Delay(cleanupInterval, cancellationToken);
            }
            catch (TaskCanceledException)
            {
                logger.LogDebug("TaskCanceledException. Exiting.");
                break;
            }
            catch (Exception ex)
            {
                logger.LogError("Task.Delay exception: {0}. Exiting.", ex.Message);
                break;
            }

            if (cancellationToken.IsCancellationRequested)
            {
                logger.LogDebug("CancellationRequested. Exiting.");
                break;
            }

            await RemoveExpiredGrantsAsync(cancellationToken);
        }
    }
    
    private async Task RemoveExpiredGrantsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            using (var serviceScope = serviceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                var tokenCleanupService = serviceScope.ServiceProvider.GetRequiredService<TokenCleanupService>();
                
                await tokenCleanupService.RemoveExpiredGrantsAsync(cancellationToken);
            }
        }
        catch (Exception ex)
        {
            logger.LogError("Exception removing expired grants: {exception}", ex.Message);
        }
    }
}