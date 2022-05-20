namespace SampleBlog.IdentityServer.EntityFramework.Storage;

internal static class Database
{
    internal static class Schemas
    {
        public const string Identity = "Identity";
    }

    internal static class Tables
    {
        public const string IdentityResource = "IdentityResources";

        public const string IdentityResourceClaim = "IdentityResourceClaims";

        public const string IdentityResourceProperty = "IdentityResourceProperties";

        public const string ApiResource = "ApiResources";

        public const string ApiResourceSecret = "ApiResourceSecrets";

        public const string ApiResourceScope = "ApiResourceScopes";

        public const string ApiResourceClaim = "ApiResourceClaims";

        public const string ApiResourceProperty = "ApiResourceProperties";

        public const string Client = "Clients";

        public const string ClientGrantType = "ClientGrantTypes";

        public const string ClientRedirectUri = "ClientRedirectUris";

        public const string ClientPostLogoutRedirectUri = "ClientPostLogoutRedirectUris";

        public const string ClientScope = "ClientScopes";

        public const string ClientSecret = "ClientSecrets";

        public const string ClientClaim = "ClientClaims";

        public const string ClientIdPRestriction = "ClientIdPRestrictions";

        public const string ClientCorsOrigin = "ClientCorsOrigins";

        public const string ClientProperty = "ClientProperties";

        public const string ApiScope = "ApiScopes";

        public const string ApiScopeClaim = "ApiScopeClaims";

        public const string ApiScopeProperty = "ApiScopeProperties";

        public const string IdentityProvider = "IdentityProviders";
    }
}