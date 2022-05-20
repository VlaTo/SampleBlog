using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using SampleBlog.IdentityServer.DependencyInjection.Options;
using SampleBlog.IdentityServer.Extensions;
using SampleBlog.IdentityServer.Hosting;
using SampleBlog.IdentityServer.Models;
using SampleBlog.IdentityServer.Services;
using SampleBlog.IdentityServer.Stores;
using SampleBlog.IdentityServer.Validation.Requests;

namespace SampleBlog.IdentityServer.Endpoints.Results;

/// <summary>
/// Result for a custom redirect
/// </summary>
/// <seealso cref="IEndpointResult" />
public class CustomRedirectResult : IEndpointResult
{
    private readonly ValidatedAuthorizeRequest request;
    private readonly string url;
    private IdentityServerOptions options;
    private IServerUrls urls;
    private IAuthorizationParametersMessageStore? authorizationParametersMessageStore;

    /// <summary>
    /// Initializes a new instance of the <see cref="CustomRedirectResult"/> class.
    /// </summary>
    /// <param name="request">The request.</param>
    /// <param name="url">The URL.</param>
    /// <exception cref="System.ArgumentNullException">
    /// request
    /// or
    /// url
    /// </exception>
    public CustomRedirectResult(ValidatedAuthorizeRequest request, string url)
    {
        if (request == null) throw new ArgumentNullException(nameof(request));
        if (url.IsMissing()) throw new ArgumentNullException(nameof(url));

        this.request = request;
        this.url = url;
    }

    internal CustomRedirectResult(
        ValidatedAuthorizeRequest request,
        string url,
        IdentityServerOptions options,
        IServerUrls urls,
        IAuthorizationParametersMessageStore? authorizationParametersMessageStore = null)
        : this(request, url)
    {
        this.options = options;
        this.urls = urls;
        this.authorizationParametersMessageStore = authorizationParametersMessageStore;
    }

    private void Init(HttpContext context)
    {
        options = options ?? context.RequestServices.GetRequiredService<IdentityServerOptions>();
        urls = urls ?? context.RequestServices.GetRequiredService<IServerUrls>();
        authorizationParametersMessageStore = authorizationParametersMessageStore ?? context.RequestServices.GetService<IAuthorizationParametersMessageStore>();
    }

    /// <summary>
    /// Executes the result.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    /// <returns></returns>
    public async Task ExecuteAsync(HttpContext context)
    {
        Init(context);

        var returnUrl = urls.BasePath.EnsureTrailingSlash() + Constants.ProtocolRoutePaths.AuthorizeCallback;

        if (null != authorizationParametersMessageStore)
        {
            var msg = new Message<IDictionary<string, string[]>>(request.ToOptimizedFullDictionary());
            
            var id = await authorizationParametersMessageStore.WriteAsync(msg);
            
            returnUrl = returnUrl.AddQueryString(Constants.AuthorizationParamsStore.MessageStoreIdParameterName, id);
        }
        else
        {
            returnUrl = returnUrl.AddQueryString(request.ToOptimizedQueryString());
        }

        if (false == url.IsLocalUrl())
        {
            // this converts the relative redirect path to an absolute one if we're 
            // redirecting to a different server
            returnUrl = urls.Origin + returnUrl;
        }

        var redirectUrl = url.AddQueryString(options.UserInteraction.CustomRedirectReturnUrlParameter, returnUrl);

        context.Response.Redirect(urls.GetAbsoluteUrl(redirectUrl));
    }
}