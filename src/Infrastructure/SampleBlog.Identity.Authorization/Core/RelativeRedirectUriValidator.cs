using SampleBlog.Identity.Authorization.Configuration;
using SampleBlog.IdentityServer.Validation;

namespace SampleBlog.Identity.Authorization.Core;

internal sealed class RelativeRedirectUriValidator : StrictRedirectUriValidator
{
    public IAbsoluteUrlFactory AbsoluteUrlFactory
    {
        get;
    }

    public RelativeRedirectUriValidator(IAbsoluteUrlFactory absoluteUrlFactory)
    {
        if (null == absoluteUrlFactory)
        {
            throw new ArgumentNullException(nameof(absoluteUrlFactory));
        }

        AbsoluteUrlFactory = absoluteUrlFactory;
    }

    public override Task<bool> IsRedirectUriValidAsync(string requestedUri, IdentityServer.Storage.Models.Client client)
    {
        if (IsLocalSPA(client))
        {
            return ValidateRelativeUris(requestedUri, client.RedirectUris);
        }

        return base.IsRedirectUriValidAsync(requestedUri, client);
    }

    public override Task<bool> IsPostLogoutRedirectUriValidAsync(string requestedUri, IdentityServer.Storage.Models.Client client)
    {
        if (IsLocalSPA(client))
        {
            return ValidateRelativeUris(requestedUri, client.PostLogoutRedirectUris);
        }

        return base.IsPostLogoutRedirectUriValidAsync(requestedUri, client);
    }

    private static bool IsLocalSPA(IdentityServer.Storage.Models.Client client) =>
        client.Properties.TryGetValue(ApplicationProfilesPropertyNames.Profile, out var clientType) &&
        ApplicationProfiles.IdentityServerSPA == clientType;

    private Task<bool> ValidateRelativeUris(string requestedUri, IEnumerable<string> clientUris)
    {
        foreach (var url in clientUris)
        {
            if (Uri.IsWellFormedUriString(url, UriKind.Relative))
            {
                var newUri = AbsoluteUrlFactory.GetAbsoluteUrl(url);

                if (string.Equals(newUri, requestedUri, StringComparison.Ordinal))
                {
                    return Task.FromResult(true);
                }
            }
        }

        return Task.FromResult(false);
    }
}