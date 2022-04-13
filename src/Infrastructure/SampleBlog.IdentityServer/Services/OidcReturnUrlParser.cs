using Microsoft.Extensions.Logging;
using SampleBlog.IdentityServer.DependencyInjection.Options;
using SampleBlog.IdentityServer.Extensions;
using SampleBlog.IdentityServer.Models;
using SampleBlog.IdentityServer.Stores;

namespace SampleBlog.IdentityServer.Services;

internal sealed class OidcReturnUrlParser : IReturnUrlParser
{
    private readonly IdentityServerOptions options;
    private readonly IAuthorizeRequestValidator validator;
    private readonly IUserSession userSession;
    private readonly IServerUrls urls;
    private readonly ILogger logger;
    private readonly IAuthorizationParametersMessageStore? authorizationParametersMessageStore;

    public OidcReturnUrlParser(
        IdentityServerOptions options,
        IAuthorizeRequestValidator validator,
        IUserSession userSession,
        IServerUrls urls,
        ILogger<OidcReturnUrlParser> logger,
        IAuthorizationParametersMessageStore? authorizationParametersMessageStore = null)
    {
        this.options = options;
        this.validator = validator;
        this.userSession = userSession;
        this.urls = urls;
        this.logger = logger;
        this.authorizationParametersMessageStore = authorizationParametersMessageStore;
    }

    public bool IsValidReturnUrl(string? returnUrl)
    {
        using var activity = Tracing.ValidationActivitySource.StartActivity("OidcReturnUrlParser.IsValidReturnUrl");

        if (options.UserInteraction.AllowOriginInReturnUrl && null != returnUrl)
        {
            if (!Uri.IsWellFormedUriString(returnUrl, UriKind.RelativeOrAbsolute))
            {
                logger.LogTrace("returnUrl is not valid");
                return false;
            }

            var host = urls.Origin;

            if (null != host && returnUrl.StartsWith(host, StringComparison.OrdinalIgnoreCase))
            {
                returnUrl = returnUrl.Substring(host.Length);
            }
        }

        if (null != returnUrl && returnUrl.IsLocalUrl())
        {
            {
                var index = returnUrl.IndexOf('?');

                if (0 <= index)
                {
                    returnUrl = returnUrl.Substring(0, index);
                }
            }

            {
                var index = returnUrl.IndexOf('#');

                if (0 <= index)
                {
                    returnUrl = returnUrl.Substring(0, index);
                }
            }

            if (returnUrl.EndsWith(Constants.ProtocolRoutePaths.Authorize, StringComparison.Ordinal) ||
                returnUrl.EndsWith(Constants.ProtocolRoutePaths.AuthorizeCallback, StringComparison.Ordinal))
            {
                logger.LogTrace("returnUrl is valid");
                return true;
            }
        }

        logger.LogTrace("returnUrl is not valid");

        return false;
    }

    public Task<AuthorizationRequest> ParseAsync(string returnUrl)
    {
        throw new NotImplementedException();
    }
}