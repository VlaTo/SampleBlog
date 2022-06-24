using IdentityModel;
using Microsoft.Extensions.Logging;
using SampleBlog.IdentityServer.Core;
using SampleBlog.IdentityServer.Extensions;
using SampleBlog.IdentityServer.Models;
using SampleBlog.IdentityServer.Validation.Contexts;
using SampleBlog.IdentityServer.Validation.Requests;
using System.Security.Claims;

namespace SampleBlog.IdentityServer.Services;

/// <summary>
/// Default claims provider implementation
/// </summary>
public class DefaultClaimsService : IClaimsService
{
    /// <summary>
    /// The logger
    /// </summary>
    protected ILogger Logger
    {
        get;
        init;
    }

    /// <summary>
    /// The user service
    /// </summary>
    protected IProfileService Profile
    {
        get;
        init;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DefaultClaimsService"/> class.
    /// </summary>
    /// <param name="profile">The profile service</param>
    /// <param name="logger">The logger</param>
    public DefaultClaimsService(IProfileService profile, ILogger<DefaultClaimsService> logger)
    {
        Logger = logger;
        Profile = profile;
    }

    /// <summary>
    /// Returns claims for an identity token
    /// </summary>
    /// <param name="subject">The subject</param>
    /// <param name="resources">The requested resources</param>
    /// <param name="includeAllIdentityClaims">Specifies if all claims should be included in the token, or if the userinfo endpoint can be used to retrieve them</param>
    /// <param name="request">The raw request</param>
    /// <returns>
    /// Claims for the identity token
    /// </returns>
    public virtual async Task<IEnumerable<Claim>> GetIdentityTokenClaimsAsync(
        ClaimsPrincipal subject,
        ResourceValidationResult resources,
        bool includeAllIdentityClaims,
        ValidatedRequest request)
    {
        using var activity = Tracing.ActivitySource.StartActivity("DefaultClaimsService.GetIdentityTokenClaims");

        Logger.LogDebug("Getting claims for identity token for subject: {subject} and client: {clientId}", subject.GetSubjectId(), request.Client?.ClientId);

        var outputClaims = new List<Claim>(GetStandardSubjectClaims(subject));

        outputClaims.AddRange(GetOptionalClaims(subject));

        // fetch all identity claims that need to go into the id token
        if (includeAllIdentityClaims || request.Client is { AlwaysIncludeUserClaimsInIdToken: true })
        {
            var additionalClaimTypes = new List<string>();

            foreach (var identityResource in resources.Resources.IdentityResources)
            {
                foreach (var userClaim in identityResource.UserClaims)
                {
                    additionalClaimTypes.Add(userClaim);
                }
            }

            // filter so we don't ask for claim types that we will eventually filter out
            var filteredClaimTypes = FilterRequestedClaimTypes(additionalClaimTypes);

            var context = new ProfileDataRequestContext(
                subject,
                request.Client!,
                IdentityServerConstants.ProfileDataCallers.ClaimsProviderIdentityToken,
                filteredClaimTypes)
            {
                RequestedResources = resources,
                ValidatedRequest = request
            };

            await Profile.GetProfileDataAsync(context);

            var claims = FilterProtocolClaims(context.IssuedClaims);

            outputClaims.AddRange(claims);
        }
        else
        {
            Logger.LogDebug("In addition to an id_token, an access_token was requested. No claims other than sub are included in the id_token. To obtain more user claims, either use the user info endpoint or set AlwaysIncludeUserClaimsInIdToken on the client configuration.");
        }

        return outputClaims;
    }

    /// <summary>
    /// Returns claims for an access token.
    /// </summary>
    /// <param name="subject">The subject.</param>
    /// <param name="resourceResult">The validated resource result</param>
    /// <param name="request">The raw request.</param>
    /// <returns>
    /// Claims for the access token
    /// </returns>
    public virtual async Task<IEnumerable<Claim>> GetAccessTokenClaimsAsync(
        ClaimsPrincipal? subject,
        ResourceValidationResult resourceResult,
        ValidatedRequest request)
    {
        using var activity = Tracing.ActivitySource.StartActivity("DefaultClaimsService.GetAccessTokenClaims");

        Logger.LogDebug("Getting claims for access token for client: {clientId}", request.Client?.ClientId);

        var outputClaims = new List<Claim>
        {
            new (JwtClaimTypes.ClientId, request.ClientId!)
        };

        // log if client ID is overwritten
        if (false == String.Equals(request.ClientId, request.Client?.ClientId))
        {
            Logger.LogDebug("Client {clientId} is impersonating {impersonatedClientId}", request.Client?.ClientId, request.ClientId);
        }

        // check for client claims
        if (0 < request.ClientClaims.Count)
        {
            if (null == subject || request.Client is { AlwaysSendClientClaims: true })
            {
                foreach (var claim in request.ClientClaims)
                {
                    var claimType = claim.Type;

                    if (false == String.IsNullOrEmpty(request.Client?.ClientClaimsPrefix))
                    {
                        claimType = request.Client.ClientClaimsPrefix + claimType;
                    }

                    outputClaims.Add(new Claim(claimType, claim.Value, claim.ValueType));
                }
            }
        }

        // add scopes (filter offline_access)
        // we use the ScopeValues collection rather than the Resources.Scopes because we support dynamic scope values 
        // from the request, so this issues those in the token.
        var scopes = resourceResult.RawScopeValues.Where(x => x != IdentityServerConstants.StandardScopes.OfflineAccess);

        foreach (var scope in scopes)
        {
            outputClaims.Add(new Claim(JwtClaimTypes.Scope, scope));
        }

        // a user is involved
        if (null != subject)
        {
            if (resourceResult.Resources.OfflineAccess)
            {
                outputClaims.Add(new Claim(JwtClaimTypes.Scope, IdentityServerConstants.StandardScopes.OfflineAccess));
            }

            Logger.LogDebug("Getting claims for access token for subject: {subject}", subject.GetSubjectId());

            outputClaims.AddRange(GetStandardSubjectClaims(subject));
            outputClaims.AddRange(GetOptionalClaims(subject));

            // fetch all resource claims that need to go into the access token
            var additionalClaimTypes = new List<string>();

            foreach (var api in resourceResult.Resources.ApiResources)
            {
                // add claims configured on api resource
                foreach (var claim in api.UserClaims)
                {
                    additionalClaimTypes.Add(claim);
                }
            }

            foreach (var scope in resourceResult.Resources.ApiScopes)
            {
                // add claims configured on scopes
                foreach (var claim in scope.UserClaims)
                {
                    additionalClaimTypes.Add(claim);
                }
            }

            // filter so we don't ask for claim types that we will eventually filter out
            additionalClaimTypes = FilterRequestedClaimTypes(additionalClaimTypes).ToList();

            var context = new ProfileDataRequestContext(
                subject,
                request.Client!,
                IdentityServerConstants.ProfileDataCallers.ClaimsProviderAccessToken,
                additionalClaimTypes.Distinct())
            {
                RequestedResources = resourceResult,
                ValidatedRequest = request
            };

            await Profile.GetProfileDataAsync(context);

            var claims = FilterProtocolClaims(context.IssuedClaims);

            outputClaims.AddRange(claims);
        }

        return outputClaims;
    }

    /// <summary>
    /// Gets the standard subject claims.
    /// </summary>
    /// <param name="subject">The subject.</param>
    /// <returns>A list of standard claims</returns>
    protected virtual IEnumerable<Claim> GetStandardSubjectClaims(ClaimsPrincipal subject)
    {
        var claims = new List<Claim>
        {
            new(JwtClaimTypes.Subject, subject.GetSubjectId()),
            new(JwtClaimTypes.AuthenticationTime, subject.GetAuthenticationTimeEpoch().ToString(), ClaimValueTypes.Integer64),
            new (JwtClaimTypes.IdentityProvider, subject.GetIdentityProvider()!)
        };

        claims.AddRange(subject.GetAuthenticationMethods());

        return claims;
    }

    /// <summary>
    /// Gets additional (and optional) claims from the cookie or incoming subject.
    /// </summary>
    /// <param name="subject">The subject.</param>
    /// <returns>Additional claims</returns>
    protected virtual IEnumerable<Claim> GetOptionalClaims(ClaimsPrincipal subject)
    {
        var claims = new List<Claim>();
        var claim = subject.FindFirst(JwtClaimTypes.AuthenticationContextClassReference);

        if (null != claim)
        {
            claims.Add(claim);
        }

        return claims;
    }

    /// <summary>
    /// Filters out protocol claims like amr, nonce etc..
    /// </summary>
    /// <param name="claimTypes">The claim types.</param>
    protected virtual IEnumerable<string> FilterRequestedClaimTypes(IReadOnlyCollection<string> claimTypes)
    {
        var claimTypesToFilter = claimTypes.Where(x => Constants.Filters.ClaimsServiceFilterClaimTypes.Contains(x));
        return claimTypes.Except(claimTypesToFilter);
    }

    /// <summary>
    /// Filters out protocol claims like amr, nonce etc..
    /// </summary>
    /// <param name="claims">The claims.</param>
    /// <returns></returns>
    protected virtual IEnumerable<Claim> FilterProtocolClaims(IReadOnlyCollection<Claim> claims)
    {
        var claimsToFilter = claims
            .Where(x => Constants.Filters.ClaimsServiceFilterClaimTypes.Contains(x.Type))
            .ToArray();

        if (0 < claimsToFilter.Length)
        {
            var types = claimsToFilter.Select(x => x.Type);
            Logger.LogDebug("Claim types from profile service that were filtered: {claimTypes}", types);
        }

        return claims.Except(claimsToFilter);
    }
}