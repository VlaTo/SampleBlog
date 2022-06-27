using IdentityModel;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using SampleBlog.IdentityServer.DependencyInjection.Options;
using SampleBlog.IdentityServer.Storage.Models;
using System.Security.Claims;
using System.Text.Json;

namespace SampleBlog.IdentityServer.Extensions;

public static class TokenExtensions
{
    /// <summary>
    /// Creates the default JWT payload dictionary
    /// </summary>
    /// <param name="token"></param>
    /// <param name="options"></param>
    /// <param name="clock"></param>
    /// <param name="logger"></param>
    /// <returns></returns>
    public static Dictionary<string, object> CreateJwtPayloadDictionary(this Token token, IdentityServerOptions options, ISystemClock clock, ILogger logger)
    {
        try
        {
            var payload = new Dictionary<string, object>
            {
                { JwtClaimTypes.Issuer, token.Issuer }
            };

            // set times (nbf, exp, iat)
            var now = clock.UtcNow - DateTimeOffset.UnixEpoch;
            var exp = now + token.Lifetime;

            payload.Add(JwtClaimTypes.NotBefore, now);
            payload.Add(JwtClaimTypes.IssuedAt, now);
            payload.Add(JwtClaimTypes.Expiration, exp);

            // add audience claim(s)
            if (token.Audiences.Any())
            {
                if (1 == token.Audiences.Count)
                {
                    payload.Add(JwtClaimTypes.Audience, token.Audiences.First());
                }
                else
                {
                    payload.Add(JwtClaimTypes.Audience, token.Audiences);
                }
            }

            // add confirmation claim (if present)
            if (null != token.Confirmation)
            {
                payload.Add(
                    JwtClaimTypes.Confirmation,
                    JsonSerializer.Deserialize<JsonElement>(token.Confirmation)
                );
            }

            // scope claims
            var scopeClaims = token.Claims
                .Where(x => x.Type == JwtClaimTypes.Scope)
                .ToArray();

            if (false == scopeClaims.IsNullOrEmpty())
            {
                var scopeValues = scopeClaims.Select(x => x.Value).ToArray();

                if (options.EmitScopesAsSpaceDelimitedStringInJwt)
                {
                    payload.Add(JwtClaimTypes.Scope, String.Join(' ', scopeValues));
                }
                else
                {
                    payload.Add(JwtClaimTypes.Scope, scopeValues);
                }
            }

            // amr claims
            var amrClaims = token.Claims
                .Where(x => x.Type == JwtClaimTypes.AuthenticationMethod)
                .ToArray();

            if (false == amrClaims.IsNullOrEmpty())
            {
                var amrValues = amrClaims.Select(x => x.Value).Distinct().ToArray();
                payload.Add(JwtClaimTypes.AuthenticationMethod, amrValues);
            }

            var simpleClaimTypes = token.Claims
                .Where(c => c.Type != JwtClaimTypes.AuthenticationMethod && c.Type != JwtClaimTypes.Scope)
                .Select(c => c.Type)
                .Distinct();

            // other claims
            foreach (var claimType in simpleClaimTypes)
            {
                // we ignore claims that are added by the above code for token verification
                if (payload.ContainsKey(claimType))
                {
                    continue;
                }

                var claims = token.Claims.Where(c => c.Type == claimType).ToArray();

                if (1 < claims.Length)
                {
                    payload.Add(claimType, AddObjects(claims));
                }
                else
                {
                    payload.Add(claimType, AddObject(claims.First()));
                }
            }

            return payload;
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "Error creating the JWT payload");
            throw;
        }
    }

    private static IEnumerable<object> AddObjects(IEnumerable<Claim> claims)
    {
        foreach (var claim in claims)
        {
            yield return AddObject(claim);
        }
    }

    private static object AddObject(Claim claim)
    {
        if (ClaimValueTypes.Boolean == claim.ValueType)
        {
            return bool.Parse(claim.Value);
        }

        if (claim.ValueType is ClaimValueTypes.Integer or ClaimValueTypes.Integer32)
        {
            return int.Parse(claim.Value);
        }

        if (ClaimValueTypes.Integer64 == claim.ValueType)
        {
            return long.Parse(claim.Value);
        }

        if (IdentityServerConstants.ClaimValueTypes.Json == claim.ValueType)
        {
            return JsonSerializer.Deserialize<JsonElement>(claim.Value);
        }

        return claim.Value;
    }
}