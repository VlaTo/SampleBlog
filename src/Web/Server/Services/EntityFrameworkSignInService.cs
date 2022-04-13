using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using SampleBlog.Core.Application.Models.Identity;
using SampleBlog.Core.Application.Responses.Identity;
using SampleBlog.Core.Application.Services;
using SampleBlog.Shared;

namespace SampleBlog.Web.Server.Services;

internal sealed class EntityFrameworkSignInService : ISignInService
{
    private readonly UserManager<BlogUser> userManager;
    private readonly SignInManager<BlogUser> signInManager;

    public EntityFrameworkSignInService(
        UserManager<BlogUser> userManager,
        SignInManager<BlogUser> signInManager)
    {
        this.userManager = userManager;
        this.signInManager = signInManager;
    }

    public Task<BlogUser?> FindUserAsync(string email)
    {
        return userManager.FindByEmailAsync(email)!;
    }

    public async Task<SignInResult> SignInAsync(BlogUser user, AuthenticationProperties? properties, string? authenticationMethod)
    {
        await signInManager.SignInAsync(user, properties, authenticationMethod);
        return SignInResult.Success;
    }

    public Task<SignInResult> ValidateCredentials(BlogUser user, string password)
    {
        return signInManager.CheckPasswordSignInAsync(user, password, false);
    }
}