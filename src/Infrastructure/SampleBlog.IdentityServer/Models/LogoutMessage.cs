using IdentityModel;
using Microsoft.IdentityModel.Tokens;
using SampleBlog.IdentityServer.Extensions;
using SampleBlog.IdentityServer.Validation.Requests;

namespace SampleBlog.IdentityServer.Models;

/// <summary>
/// Models the validated singout context.
/// </summary>
public class LogoutMessage
{
    /// <summary>
    /// Gets or sets the client identifier.
    /// </summary>
    public string? ClientId
    {
        get; 
        set;
    }

    /// <summary>
    /// Gets or sets the client name.
    /// </summary>
    public string? ClientName
    {
        get; 
        set;
    }

    /// <summary>
    /// Gets or sets the post logout redirect URI.
    /// </summary>
    public string? PostLogoutRedirectUri
    {
        get; 
        set;
    }

    /// <summary>
    /// Gets or sets the subject identifier for the user at logout time.
    /// </summary>
    public string? SubjectId { get; set; }

    /// <summary>
    /// Gets or sets the session identifier for the user at logout time.
    /// </summary>
    public string? SessionId { get; set; }

    /// <summary>
    ///  Ids of clients known to have an authentication session for user at end session time
    /// </summary>
    public IEnumerable<string>? ClientIds { get; set; }

    /// <summary>
    /// The UI locales.
    /// </summary>
    public string? UiLocales { get; set; }

    /// <summary>
    /// Gets the entire parameter collection.
    /// </summary>
    public IDictionary<string, string[]?> Parameters { get; set; } = new Dictionary<string, string[]?>();

    /// <summary>
    ///  Flag to indicate if the payload contains useful information or not to avoid serialization.
    /// </summary>
    internal bool ContainsPayload => false == ClientId?.IsNullOrEmpty() || true == ClientIds?.Any();

    /// <summary>
    /// Initializes a new instance of the <see cref="LogoutMessage"/> class.
    /// </summary>
    public LogoutMessage()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="LogoutMessage"/> class.
    /// </summary>
    /// <param name="request">The request.</param>
    public LogoutMessage(ValidatedEndSessionRequest? request)
    {
        if (null == request)
        {
            return;
        }

        if (null != request.Raw)
        {
            Parameters = request.Raw.ToFullDictionary();
        }

        // optimize params sent to logout page, since we'd like to send them in URL (not as cookie)
        Parameters.Remove(OidcConstants.EndSessionRequest.IdTokenHint);
        Parameters.Remove(OidcConstants.EndSessionRequest.PostLogoutRedirectUri);
        Parameters.Remove(OidcConstants.EndSessionRequest.State);
        Parameters.Remove(OidcConstants.AuthorizeRequest.UiLocales);

        ClientId = request.Client?.ClientId;
        ClientName = request.Client?.ClientName;
        SubjectId = request.Subject?.GetSubjectId();
        SessionId = request.SessionId;
        ClientIds = request.ClientIds;
        UiLocales = request.UiLocales;

        if (null != request.PostLogOutUri)
        {
            PostLogoutRedirectUri = request.PostLogOutUri;

            if (null != request.State)
            {
                PostLogoutRedirectUri = PostLogoutRedirectUri.AddQueryString(OidcConstants.EndSessionRequest.State, request.State);
            }
        }
    }
}