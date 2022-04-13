using System.Collections.Specialized;
using SampleBlog.IdentityServer.Extensions;

namespace SampleBlog.IdentityServer.Models;

/// <summary>
/// Models the request from a client to sign the user out.
/// </summary>
public class LogoutRequest
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
    public string? SubjectId
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the session identifier for the user at logout time.
    /// </summary>
    public string? SessionId
    {
        get;
        set;
    }

    /// <summary>
    ///  Ids of clients known to have an authentication session for user at end session time
    /// </summary>
    public IEnumerable<string>? ClientIds
    {
        get;
        set;
    }

    /// <summary>
    /// The UI locales.
    /// </summary>
    public string? UiLocales
    {
        get;
        set;
    }

    /// <summary>
    /// Gets the entire parameter collection.
    /// </summary>
    public NameValueCollection Parameters
    {
        get;
    }

    /// <summary>
    /// Gets or sets the sign out iframe URL.
    /// </summary>
    /// <value>
    /// The sign out iframe URL.
    /// </value>
    public string SignOutIFrameUrl
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets a value indicating whether the user should be prompted for signout.
    /// </summary>
    /// <value>
    ///   <c>true</c> if the signout prompt should be shown; otherwise, <c>false</c>.
    /// </value>
    public bool ShowSignOutPrompt => ClientId?.IsMissing() ?? false;

    /// <summary>
    /// Initializes a new instance of the <see cref="LogoutRequest"/> class.
    /// </summary>
    /// <param name="iframeUrl">The iframe URL.</param>
    /// <param name="message">The message.</param>
    public LogoutRequest(string iframeUrl, LogoutMessage? message)
    {
        if (null != message)
        {
            ClientId = message.ClientId;
            ClientName = message.ClientName;
            PostLogoutRedirectUri = message.PostLogoutRedirectUri;
            SubjectId = message.SubjectId;
            SessionId = message.SessionId;
            ClientIds = message.ClientIds;
            UiLocales = message.UiLocales;
            Parameters = message.Parameters.FromFullDictionary();
        }
        else
        {
            Parameters = new NameValueCollection();
        }

        SignOutIFrameUrl = iframeUrl;
    }
}