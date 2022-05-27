using MediatR;
using SampleBlog.Core.Application.Responses.Identity;
using SampleBlog.Shared;

namespace SampleBlog.Web.Identity.Core.Features.Commands.SignIn;

public sealed class SignInCommand : IRequest<IResult<TokenResponse>>
{
    public string Email
    {
        init;
        get;
    }

    public string Password
    {
        init;
        get;
    }

    public bool RememberMe
    {
        get;
    }

    public SignInCommand(string email, string password, bool rememberMe = false)
    {
        Email = email;
        Password = password;
        RememberMe = rememberMe;
    }
}