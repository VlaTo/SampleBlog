using IdentityModel;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using SampleBlog.IdentityServer.Validation.Contexts;
using SampleBlog.IdentityServer.Validation.Results;

namespace SampleBlog.IdentityServer.Validation;

/// <summary>
/// IResourceOwnerPasswordValidator that integrates with ASP.NET Identity.
/// </summary>
/// <typeparam name="TUser">The type of the user.</typeparam>
/// <seealso cref="IResourceOwnerPasswordValidator" />
public class ResourceOwnerPasswordValidator<TUser> : IResourceOwnerPasswordValidator
    where TUser : class
{
    private readonly SignInManager<TUser> signInManager;
    private readonly UserManager<TUser> userManager;
    private readonly ILogger<ResourceOwnerPasswordValidator<TUser>> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ResourceOwnerPasswordValidator{TUser}"/> class.
    /// </summary>
    /// <param name="userManager">The user manager.</param>
    /// <param name="signInManager">The sign in manager.</param>
    /// <param name="logger">The logger.</param>
    public ResourceOwnerPasswordValidator(
        UserManager<TUser> userManager,
        SignInManager<TUser> signInManager,
        ILogger<ResourceOwnerPasswordValidator<TUser>> logger)
    {
        this.userManager = userManager;
        this.signInManager = signInManager;
        this.logger = logger;
    }

    /// <summary>
    /// Validates the resource owner password credential
    /// </summary>
    /// <param name="context">The context.</param>
    /// <returns></returns>
    public virtual async Task ValidateAsync(ResourceOwnerPasswordValidationContext context)
    {
        var user = await userManager.FindByNameAsync(context.UserName);

        if (null != user)
        {
            var result = await signInManager.CheckPasswordSignInAsync(user, context.Password, true);

            if (result.Succeeded)
            {
                var userId = await userManager.GetUserIdAsync(user);

                logger.LogInformation("Credentials validated for username: {username}", context.UserName);

                context.Result = new GrantValidationResult(userId, OidcConstants.AuthenticationMethods.Password);

                return;
            }

            if (result.IsLockedOut)
            {
                logger.LogInformation("Authentication failed for username: {username}, reason: locked out", context.UserName);
            }
            else if (result.IsNotAllowed)
            {
                logger.LogInformation("Authentication failed for username: {username}, reason: not allowed", context.UserName);
            }
            else
            {
                logger.LogInformation("Authentication failed for username: {username}, reason: invalid credentials", context.UserName);
            }
        }
        else
        {
            logger.LogInformation("No user found matching username: {username}", context.UserName);
        }

        context.Result = new GrantValidationResult(TokenRequestErrors.InvalidGrant);
    }
}