using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SampleBlog.Core.Application.Configuration;
using SampleBlog.Core.Application.Services;
using SampleBlog.Infrastructure.Models.Identity;
using SignInResult = SampleBlog.Core.Application.Services.SignInResult;

namespace SampleBlog.Web.MyDinner.Core.Services;

internal sealed class SignInService : ISignInService
{
    private readonly UserManager<BlogUser> userManager;
    private readonly SignInManager<BlogUser> signInManager;
    private readonly RoleManager<BlogUserRole> roleManager;
    private readonly IOptions<ApplicationOptions> applicationOptions;

    public SignInService(
        UserManager<BlogUser> userManager,
        SignInManager<BlogUser> signInManager,
        RoleManager<BlogUserRole> roleManager,
        IOptions<ApplicationOptions> applicationOptions)
    {
        this.userManager = userManager;
        this.signInManager = signInManager;
        this.roleManager = roleManager;
        this.applicationOptions = applicationOptions;
    }

    public async Task<SignInResult> SignInAsync(string email, string password, bool rememberMe)
    {
        var user = await userManager.FindByEmailAsync(email);

        if (null == user)
        {
            return new SignInResult
            {
                IsNotFound = true
            };
        }

        if (false == user.EmailConfirmed)
        {
            return new SignInResult
            {
                IsNotAllowed = true
            };
        }

        var result = await signInManager.CanSignInAsync(user);

        if (false == result)
        {
            return new SignInResult
            {
                IsLockedOut = true
            };
        }

        var options = applicationOptions.Value;


        var signInResult = await signInManager.PasswordSignInAsync(
            user,
            password,
            options.Authentication.AllowRememberMe && rememberMe,
            options.Authentication.AllowUserLockOut
        );

        if (signInResult.Succeeded)
        {
            user.RefreshToken = GenerateRefreshToken();
            user.RefreshTokenExpiryTime = DateTime.Now.Add(options.Authentication.RefreshTokenDuration);

            var token = await GenerateJwtAsync(user);

            return new SignInResult
            {
                IsSuccess = true,
                User = user,
                Token = token
            };
        }

        /*AuthenticationProperties? properties = null;

        if (options.Authentication.AllowRememberMe && rememberMe)
        {
            var now = DateTimeOffset.UtcNow;

            properties = new AuthenticationProperties
            {
                IsPersistent = true,
                IssuedUtc = now,
                ExpiresUtc = now.Add(options.Authentication.RememberMeSignInDuration)
            };
        }

        await signInManager.SignInAsync(user, properties);*/

        return new SignInResult
        {
            IsSuccess = false,
            User = user
        };
    }

    private static string GenerateRefreshToken()
    {
        var randomNumber = new byte[32];
        using var generator = RandomNumberGenerator.Create();

        generator.GetBytes(randomNumber);

        return Convert.ToBase64String(randomNumber);
    }

    private async Task<string> GenerateJwtAsync(BlogUser user)
    {
        var options = applicationOptions.Value;
        var credentials = GetSigningCredentials();
        var claimsAsync = await GetClaimsAsync(user);
        
        return GenerateEncryptedToken(credentials, claimsAsync, options.Authentication.SecurityTokenDuration);
    }

    private SigningCredentials GetSigningCredentials()
    {
        var options = applicationOptions.Value;
        var secret = Encoding.UTF8.GetBytes(options.Authentication.Secret);
        return new SigningCredentials(new SymmetricSecurityKey(secret), SecurityAlgorithms.HmacSha256);
    }

    private async Task<IReadOnlyCollection<Claim>> GetClaimsAsync(BlogUser user)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Name, user.UserName)
        };

        if (userManager.SupportsUserEmail)
        {
            claims.Add(new(ClaimTypes.Email, user.Email));
        }

        if (userManager.SupportsUserPhoneNumber && false == String.IsNullOrEmpty(user.PhoneNumber))
        {
            claims.Add(new(ClaimTypes.MobilePhone, user.PhoneNumber));
        }

        if (userManager.SupportsUserRole)
        {
            var permissions = new List<Claim>();
            var roleNames = await userManager.GetRolesAsync(user);

            foreach (var roleName in roleNames)
            {
                var userRole = await roleManager.FindByNameAsync(roleName);
                var userRoleClaims = await roleManager.GetClaimsAsync(userRole);

                claims.Add(new Claim(ClaimTypes.Role, roleName));
                permissions.AddRange(userRoleClaims);
            }

            claims.AddRange(permissions);
        }

        if (userManager.SupportsUserClaim)
        {
            var userClaims = await userManager.GetClaimsAsync(user);
            claims.AddRange(userClaims);
        }

        return claims;
    }

    private static string GenerateEncryptedToken(SigningCredentials signingCredentials, IEnumerable<Claim> claims, TimeSpan duration)
    {
        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.UtcNow.Add(duration),
            signingCredentials: signingCredentials
        );
        var tokenHandler = new JwtSecurityTokenHandler();

        return tokenHandler.WriteToken(token);
    }
}