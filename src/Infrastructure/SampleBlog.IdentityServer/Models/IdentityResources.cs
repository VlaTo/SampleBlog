using IdentityModel;
using SampleBlog.IdentityServer.Storage.Models;

namespace SampleBlog.IdentityServer.Models;

/// <summary>
/// Convenience class that defines standard identity resources.
/// </summary>
public static class IdentityResources
{
    #region OpenId

    /// <summary>
    /// Models the standard openid scope
    /// </summary>
    /// <seealso cref="IdentityResource" />
    public class OpenId : IdentityResource
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OpenId"/> class.
        /// </summary>
        public OpenId()
            : base(IdentityServerConstants.StandardScopes.OpenId)
        {
            //Name = IdentityServerConstants.StandardScopes.OpenId;
            DisplayName = "Your user identifier";
            Required = true;
            UserClaims.Add(JwtClaimTypes.Subject);
        }
    }

    #endregion

    #region Profile

    /// <summary>
    /// Models the standard profile scope
    /// </summary>
    /// <seealso cref="IdentityResource" />
    public class Profile : IdentityResource
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Profile"/> class.
        /// </summary>
        public Profile()
            : base(IdentityServerConstants.StandardScopes.Profile)
        {
            //Name = IdentityServerConstants.StandardScopes.Profile;
            DisplayName = "User profile";
            Description = "Your user profile information (first name, last name, etc.)";
            Emphasize = true;
            UserClaims = Constants.ScopeToClaimsMapping[IdentityServerConstants.StandardScopes.Profile].ToList();
        }
    }

    #endregion
}