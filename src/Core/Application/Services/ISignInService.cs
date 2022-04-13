using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using SampleBlog.Core.Application.Models.Identity;

namespace SampleBlog.Core.Application.Services;

public interface ISignInService
{
    Task<BlogUser?> FindUserAsync(string email);

    Task<SignInResult> SignInAsync(BlogUser user, AuthenticationProperties? properties, string? authenticationMethod);

    Task<SignInResult> ValidateCredentials(BlogUser user, string password);
}