using System.Security.Claims;
using SampleBlog.IdentityServer.Models;
using SampleBlog.IdentityServer.Storage.Models;
using SampleBlog.IdentityServer.Validation.Requests;

namespace SampleBlog.IdentityServer.Validation.Contexts;

/// <summary>
/// Class describing the profile data request
/// </summary>
public class ProfileDataRequestContext
{
    /// <summary>
    /// Gets or sets the validatedRequest.
    /// </summary>
    /// <value>
    /// The validatedRequest.
    /// </value>
    public ValidatedRequest ValidatedRequest
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the subject.
    /// </summary>
    /// <value>
    /// The subject.
    /// </value>
    public ClaimsPrincipal Subject
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the requested claim types.
    /// </summary>
    /// <value>
    /// The requested claim types.
    /// </value>
    public IReadOnlyCollection<string> RequestedClaimTypes
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the client id.
    /// </summary>
    /// <value>
    /// The client id.
    /// </value>
    public Client Client
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the caller.
    /// </summary>
    /// <value>
    /// The caller.
    /// </value>
    public string Caller
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the requested resources (if available).
    /// </summary>
    /// <value>
    /// The resources.
    /// </value>
    public ResourceValidationResult RequestedResources
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the issued claims.
    /// </summary>
    /// <value>
    /// The issued claims.
    /// </value>
    public List<Claim> IssuedClaims
    {
        get;
        set;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ProfileDataRequestContext"/> class.
    /// </summary>
    public ProfileDataRequestContext()
    {
        IssuedClaims = new List<Claim>();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ProfileDataRequestContext" /> class.
    /// </summary>
    /// <param name="subject">The subject.</param>
    /// <param name="client">The client.</param>
    /// <param name="caller">The caller.</param>
    /// <param name="requestedClaimTypes">The requested claim types.</param>
    public ProfileDataRequestContext(
        ClaimsPrincipal subject,
        Client client,
        string caller,
        IEnumerable<string> requestedClaimTypes)
        : this()
    {
        Subject = subject;
        Client = client;
        Caller = caller;
        RequestedClaimTypes = requestedClaimTypes.ToArray();
    }
}