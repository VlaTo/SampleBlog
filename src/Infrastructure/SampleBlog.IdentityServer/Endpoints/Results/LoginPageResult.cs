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
/// Result for login page
/// </summary>
/// <seealso cref="IEndpointResult" />
public class LoginPageResult : IEndpointResult
{
    private readonly ValidatedAuthorizeRequest request;
    private IdentityServerOptions options;
    private IServerUrls urls;
    private IAuthorizationParametersMessageStore? authorizationParametersMessageStore;

    /// <summary>
    /// Initializes a new instance of the <see cref="LoginPageResult"/> class.
    /// </summary>
    /// <param name="request">The request.</param>
    /// <exception cref="System.ArgumentNullException">request</exception>
    public LoginPageResult(ValidatedAuthorizeRequest request)
    {
        this.request = request ?? throw new ArgumentNullException(nameof(request));
    }

    internal LoginPageResult(
        ValidatedAuthorizeRequest request,
        IdentityServerOptions options,
        IServerUrls urls,
        IAuthorizationParametersMessageStore? authorizationParametersMessageStore = null)
        : this(request)
    {
        this.options = options;
        this.urls = urls;
        this.authorizationParametersMessageStore = authorizationParametersMessageStore;
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

        var loginUrl = options.UserInteraction.LoginUrl;

        if (false == loginUrl.IsLocalUrl())
        {
            // this converts the relative redirect path to an absolute one if we're 
            // redirecting to a different server
            returnUrl = urls.Origin + returnUrl;
        }

        var url = loginUrl.AddQueryString(options.UserInteraction.LoginReturnUrlParameter, returnUrl);

        context.Response.Redirect(urls.GetAbsoluteUrl(url));
    }

    private void Init(HttpContext context)
    {
        options = options ?? context.RequestServices.GetRequiredService<IdentityServerOptions>();
        urls = urls ?? context.RequestServices.GetRequiredService<IServerUrls>();
        authorizationParametersMessageStore = authorizationParametersMessageStore ?? context.RequestServices.GetService<IAuthorizationParametersMessageStore>();
    }
}