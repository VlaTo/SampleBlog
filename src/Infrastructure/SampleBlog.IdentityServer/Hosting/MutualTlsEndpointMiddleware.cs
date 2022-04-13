using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SampleBlog.IdentityServer.DependencyInjection.Options;
using SampleBlog.IdentityServer.Extensions;

namespace SampleBlog.IdentityServer.Hosting;

/// <summary>
///     Middleware for re-writing the MTLS enabled endpoints to the standard protocol endpoints
/// </summary>
public class MutualTlsEndpointMiddleware
{
    private readonly ILogger<MutualTlsEndpointMiddleware> logger;
    private readonly RequestDelegate next;
    private readonly IdentityServerOptions options;

    /// <summary>
    ///     ctor
    /// </summary>
    /// <param name="next"></param>
    /// <param name="options"></param>
    /// <param name="logger"></param>
    public MutualTlsEndpointMiddleware(
        RequestDelegate next,
        IdentityServerOptions options,
        ILogger<MutualTlsEndpointMiddleware> logger)
    {
        this.next = next;
        this.options = options;
        this.logger = logger;
    }

    public async Task Invoke(HttpContext context, IAuthenticationSchemeProvider schemes)
    {
        const string dot = ".";

        if (options.MutualTls.Enabled)
        {
            // domain-based MTLS
            if (options.MutualTls.DomainName.IsPresent())
            {
                // separate domain
                if (options.MutualTls.DomainName.Contains(dot))
                {
                    if (context.Request.Host.Host.Equals(options.MutualTls.DomainName, StringComparison.OrdinalIgnoreCase))
                    {
                        var result = await TriggerCertificateAuthentication(context);
                        
                        if (false == result.Succeeded)
                        {
                            return;
                        }
                    }
                }
                // sub-domain
                else
                {
                    if (context.Request.Host.Host.StartsWith(options.MutualTls.DomainName + dot, StringComparison.OrdinalIgnoreCase))
                    {
                        var result = await TriggerCertificateAuthentication(context);

                        if (false == result.Succeeded)
                        {
                            return;
                        }
                    }
                }
            }
            // path based MTLS
            else if (context.Request.Path.StartsWithSegments(Constants.ProtocolRoutePaths.MtlsPathPrefix.EnsureLeadingSlash(), out var subPath))
            {
                var result = await TriggerCertificateAuthentication(context);

                if (result.Succeeded)
                {
                    var path = Constants.ProtocolRoutePaths.ConnectPathPrefix + subPath.ToString().EnsureLeadingSlash();
                    
                    path = path.EnsureLeadingSlash();

                    logger.LogDebug("Rewriting MTLS request from: {oldPath} to: {newPath}", context.Request.Path.ToString(), path);

                    context.Request.Path = path;
                }
                else
                {
                    return;
                }
            }
        }

        await next(context);
    }

    private async Task<AuthenticateResult> TriggerCertificateAuthentication(HttpContext context)
    {
        var x509AuthResult = await context.AuthenticateAsync(options.MutualTls.ClientCertificateAuthenticationScheme);

        if (false == x509AuthResult.Succeeded)
        {
            logger.LogDebug("MTLS authentication failed, error: {error}.", x509AuthResult.Failure?.Message);

            await context.ForbidAsync(options.MutualTls.ClientCertificateAuthenticationScheme);
        }

        return x509AuthResult;
    }
}