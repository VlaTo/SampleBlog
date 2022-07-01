using System.Net;
using IdentityModel;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;
using SampleBlog.IdentityServer.Extensions;
using SampleBlog.IdentityServer.Hosting;

namespace SampleBlog.IdentityServer.Endpoints.Results;

internal sealed class ProtectedResourceErrorResult : IEndpointResult
{
    public string Error
    {
        get;
        private set;
    }

    public string? ErrorDescription
    {
        get;
        private set;
    }

    public ProtectedResourceErrorResult(string error, string? errorDescription = null)
    {
        Error = error;
        ErrorDescription = errorDescription;
    }

    public Task ExecuteAsync(HttpContext context)
    {
        context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
        context.Response.SetNoCache();

        if (Constants.ProtectedResourceErrorStatusCodes.ContainsKey(Error))
        {
            context.Response.StatusCode = Constants.ProtectedResourceErrorStatusCodes[Error];
        }

        if (Error == OidcConstants.ProtectedResourceErrors.ExpiredToken)
        {
            Error = OidcConstants.ProtectedResourceErrors.InvalidToken;
            ErrorDescription = "The access token expired";
        }

        var errorString = string.Format($"error=\"{Error}\"");

        if (ErrorDescription.IsMissing())
        {
            context.Response.Headers.Add(HeaderNames.WWWAuthenticate, new StringValues(new[] { "Bearer realm=\"IdentityServer\"", errorString }).ToString());
        }
        else
        {
            var errorDescriptionString = string.Format($"error_description=\"{ErrorDescription}\"");
            context.Response.Headers.Add(HeaderNames.WWWAuthenticate, new StringValues(new[] { "Bearer realm=\"IdentityServer\"", errorString, errorDescriptionString }).ToString());
        }

        return Task.CompletedTask;
    }
}