using Microsoft.Extensions.Logging;

namespace SampleBlog.IdentityServer.Services;

public sealed class CorsPolicyService : ICorsPolicyService
{
    private readonly IServiceProvider provider;
    private readonly ILogger<CorsPolicyService> logger;

    public CorsPolicyService(
        IServiceProvider provider,
        ILogger<CorsPolicyService> logger)
    {
        this.provider = provider;
        this.logger = logger;
    }

    public Task<bool> IsOriginAllowedAsync(string origin)
    {
        origin = origin.ToLowerInvariant();

        // doing this here and not in the ctor because: https://github.com/aspnet/CORS/issues/105
        /*var dbContext = provider.GetRequiredService<IConfigurationDbContext>();

        var query = from o in dbContext.ClientCorsOrigins
            where o.Origin == origin
            select o;

        var isAllowed = await query.AnyAsync(CancellationTokenProvider.CancellationToken);

        Logger.LogDebug("Origin {origin} is allowed: {originAllowed}", origin, isAllowed);

        return isAllowed;*/

        return Task.FromResult(true);
    }
}