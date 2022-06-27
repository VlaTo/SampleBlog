using System.Net;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http;
using SampleBlog.IdentityServer.Extensions;
using SampleBlog.IdentityServer.Hosting;
using SampleBlog.IdentityServer.ResponseHandling.Models;

namespace SampleBlog.IdentityServer.Endpoints.Results;

public sealed class TokenErrorResult : IEndpointResult
{
    public TokenErrorResponse Response
    {
        get;
        init;
    }

    public TokenErrorResult(TokenErrorResponse error)
    {
        Response = error;
    }

    public async Task ExecuteAsync(HttpContext context)
    {
        context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
        context.Response.SetNoCache();

        var dto = new Result
        {
            error = Response.Error,
            error_description = Response.ErrorDescription,

            custom = Response.Custom
        };

        await context.Response.WriteJsonAsync(dto);
    }

    /// <summary>
    /// 
    /// </summary>
    internal class Result
    {
        public string error
        {
            get;
            set;
        }

        public string error_description
        {
            get;
            set;
        }

        [JsonExtensionData]
        public Dictionary<string, object> custom
        {
            get;
            set;
        }
    }
}