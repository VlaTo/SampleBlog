using System.Security.Claims;
using IdentityModel;

namespace SampleBlog.IdentityServer;

/// <summary>
/// Model properties of an IdentityServer user
/// </summary>
public class IdentityServerUser
{
    /// <summary>
    /// Subject ID (mandatory)
    /// </summary>
    public string SubjectId
    {
        get;
    }

    /// <summary>
    /// Display name (optional)
    /// </summary>
    public string DisplayName
    {
        get;
        set;
    }

    /// <summary>
    /// Identity provider (optional)
    /// </summary>
    public string IdentityProvider
    {
        get;
        set;
    }

    /// <summary>
    /// Tenant (optional)
    /// </summary>
    public string Tenant
    {
        get;
        set;
    }

    /// <summary>
    /// Authentication methods
    /// </summary>
    public ICollection<string> AuthenticationMethods
    {
        get;
        set;
    }

    /// <summary>
    /// Authentication time
    /// </summary>
    public DateTime? AuthenticationTime
    {
        get;
        set;
    }

    /// <summary>
    /// Additional claims
    /// </summary>
    public ICollection<Claim> AdditionalClaims
    {
        get;
        set;
    }

    /// <summary>
    /// Initializes a user identity
    /// </summary>
    /// <param name="subjectId">The subject ID</param>
    public IdentityServerUser(string subjectId)
    {
        SubjectId = subjectId;
        AuthenticationMethods = new HashSet<string>();
        AdditionalClaims = new HashSet<Claim>(new ClaimComparer());
    }

    /// <summary>
    /// Creates an IdentityServer claims principal
    /// </summary>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public ClaimsPrincipal CreatePrincipal()
    {
        if (String.IsNullOrEmpty(SubjectId))
        {
            throw new ArgumentException("SubjectId is mandatory", nameof(SubjectId));
        }

        var claims = new List<Claim>
        {
            new(JwtClaimTypes.Subject, SubjectId)
        };

        if (false == String.IsNullOrEmpty(DisplayName))
        {
            claims.Add(new Claim(JwtClaimTypes.Name, DisplayName));
        }

        if (false == String.IsNullOrEmpty(IdentityProvider))
        {
            claims.Add(new Claim(JwtClaimTypes.IdentityProvider, IdentityProvider));
        }

        if (false == String.IsNullOrEmpty(Tenant))
        {
            claims.Add(new Claim(IdentityServerConstants.ClaimTypes.Tenant, Tenant));
        }

        if (AuthenticationTime.HasValue)
        {
            var time = new DateTimeOffset(AuthenticationTime.Value).ToUnixTimeSeconds().ToString();
            claims.Add(new Claim(JwtClaimTypes.AuthenticationTime, time));
        }

        if (AuthenticationMethods.Any())
        {
            foreach (var amr in AuthenticationMethods)
            {
                claims.Add(new Claim(JwtClaimTypes.AuthenticationMethod, amr));
            }
        }

        claims.AddRange(AdditionalClaims);

        var id = new ClaimsIdentity(
            claims.Distinct(new ClaimComparer()),
            Constants.IdentityServerAuthenticationType,
            JwtClaimTypes.Name,
            JwtClaimTypes.Role
        );

        return new ClaimsPrincipal(id);
    }
}