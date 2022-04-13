using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SampleBlog.IdentityServer.Core;
using SampleBlog.IdentityServer.DependencyInjection.Options;
using SampleBlog.IdentityServer.Extensions;

namespace SampleBlog.IdentityServer.Services;

internal sealed class CorsPolicyProvider : ICorsPolicyProvider
{
    private readonly ILogger logger;
    private readonly ICorsPolicyProvider instance;
    private readonly IServiceProvider provider;
    private readonly IdentityServerOptions options;

    public CorsPolicyProvider(
        Decorator<ICorsPolicyProvider> inner,
        IdentityServerOptions options,
        IServiceProvider provider,
        ILogger<CorsPolicyProvider> logger)
    {
        instance = inner.Instance;
        this.options = options;
        this.provider = provider;
        this.logger = logger;
    }

    public Task<CorsPolicy?> GetPolicyAsync(HttpContext context, string? policyName)
    {
        if (options.Cors.CorsPolicyName == policyName)
        {
            return ProcessAsync(context);
        }

        return instance.GetPolicyAsync(context, policyName);
    }
    
    private async Task<CorsPolicy?> ProcessAsync(HttpContext context)
    {
        var origin = context.Request.GetCorsOrigin();

        if (null != origin)
        {
            var path = context.Request.Path;

            if (IsPathAllowed(path))
            {
                logger.LogDebug("CORS request made for path: {path} from origin: {origin}", path, origin);

                // manually resolving this from DI because this: 
                // https://github.com/aspnet/CORS/issues/105
                var corsPolicyService = provider.GetRequiredService<ICorsPolicyService>();

                if (await corsPolicyService.IsOriginAllowedAsync(origin))
                {
                    logger.LogDebug("CorsPolicyService allowed origin: {origin}", origin);
                    return Allow(origin);
                }

                logger.LogWarning("CorsPolicyService did not allow origin: {origin}", origin);
            }
            else
            {
                logger.LogDebug("CORS request made for path: {path} from origin: {origin} but was ignored because path was not for an allowed IdentityServer CORS endpoint", path, origin);
            }
        }

        return null;
    }

    private bool IsPathAllowed(PathString path) => options.Cors.CorsPaths.Any(x => String.Equals(path, x));

    private CorsPolicy Allow(string origin)
    {
        var policyBuilder = new CorsPolicyBuilder()
            .WithOrigins(origin)
            .AllowAnyHeader()
            .AllowAnyMethod();

        if (options.Cors.PreflightCacheDuration.HasValue)
        {
            policyBuilder.SetPreflightMaxAge(options.Cors.PreflightCacheDuration.Value);
        }

        return policyBuilder.Build();
    }
}