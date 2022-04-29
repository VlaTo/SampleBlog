using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using SampleBlog.Identity.Authorization.Configuration;
using SampleBlog.Identity.Authorization.Options;
using SampleBlog.IdentityServer.Services;

namespace SampleBlog.Identity.Authorization.Core;

internal sealed class DefaultClientRequestParametersProvider : IClientRequestParametersProvider
{
    private readonly IIssuerNameService issuerNameService;

    public IAbsoluteUrlFactory UrlFactory
    {
        get;
    }

    public IOptions<ApiAuthorizationOptions> Options
    {
        get;
    }

    public DefaultClientRequestParametersProvider(
        IIssuerNameService issuerNameService,
        IAbsoluteUrlFactory urlFactory,
        IOptions<ApiAuthorizationOptions> options)
    {
        this.issuerNameService = issuerNameService;

        UrlFactory = urlFactory;
        Options = options;
    }

    public async Task<IDictionary<string, string>> GetClientParametersAsync(HttpContext context, string clientId)
    {
        var authority = await issuerNameService.GetCurrentAsync();
        var client = Options.Value.Clients[clientId];

        if (false == client.Properties.TryGetValue(ApplicationProfilesPropertyNames.Profile, out var type))
        {
            throw new InvalidOperationException($"Can't determine the type for the client '{clientId}'");
        }

        string responseType;

        switch (type)
        {
            case ApplicationProfiles.IdentityServerSPA:
            case ApplicationProfiles.SPA:
            case ApplicationProfiles.NativeApp:
            {
                responseType = SupportedResponseTypes.Code;
                break;
            }

            default:
            {
                throw new InvalidOperationException($"Invalid application type '{type}' for '{clientId}'.");
            }
        }

        var parameters = new Dictionary<string, string>
        {
            ["authority"] = authority,
            ["client_id"] = client.ClientId,
            ["redirect_uri"] = UrlFactory.GetAbsoluteUrl(context, client.RedirectUris.First()),
            ["post_logout_redirect_uri"] = UrlFactory.GetAbsoluteUrl(context, client.PostLogoutRedirectUris.First()),
            ["response_type"] = responseType,
            ["scope"] = String.Join(' ', client.AllowedScopes)
        };

        return parameters;
    }
}