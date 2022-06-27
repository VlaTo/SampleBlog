using IdentityModel;
using Microsoft.AspNetCore.Http;
using SampleBlog.IdentityServer.Extensions;
using SampleBlog.IdentityServer.Hosting;
using SampleBlog.IdentityServer.ResponseHandling.Models;
using System.Text.Json.Serialization;

namespace SampleBlog.IdentityServer.Endpoints.Results;

internal sealed class TokenResult : IEndpointResult
{
    public TokenResponse Response
    {
        get;
        set;
    }

    public TokenResult(TokenResponse response)
    {
        Response = response ?? throw new ArgumentNullException(nameof(response));
    }

    public async Task ExecuteAsync(HttpContext context)
    {
        context.Response.SetNoCache();

        var result = new Result
        {
            IdToken = Response.IdentityToken,
            AccessToken = Response.AccessToken,
            RefreshToken = Response.RefreshToken,
            ExpiresIn = Response.AccessTokenLifetime,
            TokenType = OidcConstants.TokenResponse.BearerTokenType,
            Scope = Response.Scope,
            Custom = Response.Custom
        };

        await context.Response.WriteJsonAsync(result);
    }

    /// <summary>
    /// 
    /// </summary>
    internal sealed class Result
    {
        [JsonPropertyName("id_token")]
        public string? IdToken
        {
            get;
            init;
        }

        [JsonPropertyName("access_token")]
        public string AccessToken
        {
            get;
            init;
        }

        [JsonIgnore]
        public TimeSpan ExpiresIn
        {
            get;
            init;
        }

        [JsonPropertyName("expires_in")]
        public int Expires => Convert.ToInt32(ExpiresIn.TotalMilliseconds);

        [JsonPropertyName("token_type")]
        public string TokenType
        {
            get;
            init;
        }

        [JsonPropertyName("refresh_token")]
        public string? RefreshToken
        {
            get;
            init;
        }

        [JsonPropertyName("scope")]
        public string Scope
        {
            get;
            init;
        }

        [JsonExtensionData]
        public Dictionary<string, object>? Custom
        {
            get;
            init;
        }
    }
}