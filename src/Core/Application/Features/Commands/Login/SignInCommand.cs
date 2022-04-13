using MediatR;
using SampleBlog.Core.Application.Requests.Identity;
using SampleBlog.Core.Application.Responses.Identity;
using SampleBlog.Shared;

namespace SampleBlog.Core.Application.Features.Commands.Login;

public sealed class SignInCommand : IRequest<IResult<TokenResponse>>
{
    public SignInRequest Request
    {
        get;
    }

    public SignInCommand(SignInRequest request)
    {
        Request = request;
    }
}