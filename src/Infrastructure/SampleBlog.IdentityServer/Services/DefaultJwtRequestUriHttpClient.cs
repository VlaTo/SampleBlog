using IdentityModel;
using Microsoft.Extensions.Logging;
using SampleBlog.IdentityServer.Core;
using SampleBlog.IdentityServer.DependencyInjection.Options;
using SampleBlog.IdentityServer.Storage.Models;
using SampleBlog.IdentityServer.Storage.Services;

namespace SampleBlog.IdentityServer.Services;

/// <summary>
/// Default JwtRequest client
/// </summary>
public class DefaultJwtRequestUriHttpClient : IJwtRequestUriHttpClient
{
    private readonly HttpClient httpClient;
    private readonly IdentityServerOptions options;
    private readonly ILogger<DefaultJwtRequestUriHttpClient> logger;
    private readonly ICancellationTokenProvider cancellationTokenProvider;

    /// <summary>
    /// ctor
    /// </summary>
    /// <param name="httpClient">An HTTP client</param>
    /// <param name="options">The options.</param>
    /// <param name="loggerFactory">The logger factory</param>
    /// <param name="cancellationTokenProvider"></param>
    public DefaultJwtRequestUriHttpClient(
        HttpClient httpClient,
        IdentityServerOptions options,
        ICancellationTokenProvider cancellationTokenProvider,
        ILoggerFactory loggerFactory)
    {
        this.httpClient = httpClient;
        this.options = options;
        this.cancellationTokenProvider = cancellationTokenProvider;

        logger = loggerFactory.CreateLogger<DefaultJwtRequestUriHttpClient>();
    }

    /// <inheritdoc />
    public async Task<string?> GetJwtAsync(string url, Client client)
    {
        using var activity = Tracing.ServiceActivitySource.StartActivity("DefaultJwtRequestUriHttpClient.GetJwt");

        var req = new HttpRequestMessage(HttpMethod.Get, url);
        
        req.Options.TryAdd(IdentityServerConstants.JwtRequestClientKey, client);

        var response = await httpClient.SendAsync(req, cancellationTokenProvider.CancellationToken);

        if (System.Net.HttpStatusCode.OK == response.StatusCode)
        {
            if (options.StrictJarValidation)
            {
                var mediaType = response.Content.Headers.ContentType.MediaType;
                var jwtRequestType = $"application/{JwtClaimTypes.JwtTypes.AuthorizationRequest}";

                if (false == String.Equals(mediaType, jwtRequestType, StringComparison.Ordinal))
                {
                    logger.LogError("Invalid content type {type} from jwt url {url}", mediaType, url);
                    return null;
                }
            }

            logger.LogDebug("Success http response from jwt url {url}", url);

            var json = await response.Content.ReadAsStringAsync();

            return json;
        }

        logger.LogError("Invalid http status code {status} from jwt url {url}", response.StatusCode, url);

        return null;
    }
}