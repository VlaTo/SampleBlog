using System.Security.Claims;
using System.Text.Json;
using IdentityModel;

namespace SampleBlog.IdentityServer.Extensions;

internal static class ClaimsExtensions
{
    public static Dictionary<string, object> ToClaimsDictionary(this IEnumerable<Claim>? claims)
    {
        var dictionary = new Dictionary<string, object>();

        if (null != claims)
        {
            var distinctClaims = claims.Distinct(new ClaimComparer());

            foreach (var claim in distinctClaims)
            {
                if (false == dictionary.TryGetValue(claim.Type, out var value))
                {
                    dictionary.Add(claim.Type, GetValue(claim));
                    continue;
                }

                //var value = dictionary[claim.Type];

                if (value is List<object> list)
                {
                    list.Add(GetValue(claim));
                }
                else
                {
                    dictionary.Remove(claim.Type);
                    dictionary.Add(claim.Type, new List<object> { value, GetValue(claim) });
                }
            }
        }

        return dictionary;
    }

    private static object GetValue(Claim claim)
    {
        if (claim.ValueType == ClaimValueTypes.Integer ||
            claim.ValueType == ClaimValueTypes.Integer32)
        {
            if (Int32.TryParse(claim.Value, out int value))
            {
                return value;
            }
        }

        if (claim.ValueType == ClaimValueTypes.Integer64)
        {
            if (Int64.TryParse(claim.Value, out long value))
            {
                return value;
            }
        }

        if (claim.ValueType == ClaimValueTypes.Boolean)
        {
            if (bool.TryParse(claim.Value, out bool value))
            {
                return value;
            }
        }

        if (claim.ValueType == IdentityServerConstants.ClaimValueTypes.Json)
        {
            try
            {
                return JsonSerializer.Deserialize<JsonElement>(claim.Value);
            }
            catch { }
        }

        return claim.Value;
    }
}