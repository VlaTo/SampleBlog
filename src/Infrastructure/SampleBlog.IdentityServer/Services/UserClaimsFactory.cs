using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using IdentityModel;
using SampleBlog.IdentityServer.Core;

namespace SampleBlog.IdentityServer.Services;

internal sealed class UserClaimsFactory<TUser> : IUserClaimsPrincipalFactory<TUser>
    where TUser : class
{
    private readonly Decorator<IUserClaimsPrincipalFactory<TUser>> factory;
    private readonly UserManager<TUser> userManager;

    public UserClaimsFactory(
        Decorator<IUserClaimsPrincipalFactory<TUser>> factory,
        UserManager<TUser> userManager)
    {
        this.factory = factory;
        this.userManager = userManager;
    }

    public async Task<ClaimsPrincipal> CreateAsync(TUser user)
    {
        var principal = await factory.Instance.CreateAsync(user);
        var identity = principal.Identities.First();

        if (false == identity.HasClaim(claim => String.Equals(claim.Type, JwtClaimTypes.Subject)))
        {
            var userId = await userManager.GetUserIdAsync(user);
            identity.AddClaim(new Claim(JwtClaimTypes.Subject, userId));
        }

        var username = await userManager.GetUserNameAsync(user);
        var usernameClaim = identity.FindFirst(
            claim => String.Equals(claim.Type, userManager.Options.ClaimsIdentity.UserNameClaimType) && String.Equals(claim.Value, username)
        );
        
        if (null != usernameClaim)
        {
            identity.RemoveClaim(usernameClaim);
            identity.AddClaim(new Claim(JwtClaimTypes.PreferredUserName, username));
        }

        if (false == identity.HasClaim(claim => String.Equals(claim.Type, JwtClaimTypes.Name)))
        {
            identity.AddClaim(new Claim(JwtClaimTypes.Name, username));
        }

        if (userManager.SupportsUserEmail)
        {
            var email = await userManager.GetEmailAsync(user);

            if (false == String.IsNullOrWhiteSpace(email))
            {
                var verified = await userManager.IsEmailConfirmedAsync(user) ? "true" : "false";
                var claim = new Claim(JwtClaimTypes.EmailVerified, verified, ClaimValueTypes.Boolean);

                identity.AddClaim(claim);
            }
        }

        if (userManager.SupportsUserPhoneNumber)
        {
            var phoneNumber = await userManager.GetPhoneNumberAsync(user);

            if (false == String.IsNullOrWhiteSpace(phoneNumber))
            {
                var verified = await userManager.IsPhoneNumberConfirmedAsync(user) ? "true" : "false";

                identity.AddClaims(new[]
                {
                    new Claim(JwtClaimTypes.PhoneNumber, phoneNumber, ClaimValueTypes.String),
                    new Claim(JwtClaimTypes.PhoneNumberVerified, verified, ClaimValueTypes.Boolean)
                });
            }
        }

        return principal;
    }
}