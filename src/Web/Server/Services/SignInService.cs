using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SampleBlog.Core.Application.Configuration;
using SampleBlog.Core.Application.Models.Identity;
using SampleBlog.Core.Application.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace SampleBlog.Web.Server.Services;

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

    public async Task<Core.Application.Services.SignInResult> SignInAsync(string email, string password, bool rememberMe)
    {
        var user = await userManager.FindByEmailAsync(email);

        if (null == user)
        {
            return new Core.Application.Services.SignInResult
            {
                IsNotFound = true
            };
        }

        if (false == user.EmailConfirmed)
        {
            return new Core.Application.Services.SignInResult
            {
                IsNotAllowed = true
            };
        }

        var result = await signInManager.CanSignInAsync(user);

        if (false == result)
        {
            return new Core.Application.Services.SignInResult
            {
                IsLockedOut = true
            };
        }

        var options = applicationOptions.Value;
        AuthenticationProperties? properties = null;

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

        await signInManager.SignInAsync(user, properties, "Local");

        user.RefreshToken = GenerateRefreshToken();
        user.RefreshTokenExpiryTime = DateTime.Now.Add(options.Authentication.RefreshTokenDuration);

        var token = await GenerateJwtAsync(user);

        return new Core.Application.Services.SignInResult
        {
            IsSuccess = true,
            User = user,
            Token = token
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

    private async Task<IEnumerable<Claim>> GetClaimsAsync(BlogUser user)
    {
        var roleNames = await userManager.GetRolesAsync(user);
        var roleClaims = new List<Claim>();
        var permissionClaims = new List<Claim>();

        foreach (var roleName in roleNames)
        {
            roleClaims.Add(new Claim(ClaimTypes.Role, roleName));

            var userRole = await roleManager.FindByNameAsync(roleName);
            var claims = await roleManager.GetClaimsAsync(userRole);

            permissionClaims.AddRange(claims);
        }

        var userClaims = await userManager.GetClaimsAsync(user);

        return new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id),
                new(ClaimTypes.Email, user.Email),
                new(ClaimTypes.Name, user.UserName),
                new(ClaimTypes.MobilePhone, user.PhoneNumber ?? String.Empty)
            }
            .Union(userClaims)
            .Union(roleClaims)
            .Union(permissionClaims);
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