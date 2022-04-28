using MediatR;
using Microsoft.Extensions.Options;
using SampleBlog.Core.Application.Configuration;
using SampleBlog.Core.Application.Responses.Identity;
using SampleBlog.Core.Application.Services;
using SampleBlog.Shared;
using SignInResult = SampleBlog.Core.Application.Services.SignInResult;

namespace SampleBlog.Core.Application.Features.Commands.Login;

public sealed class SignInRequestHandler : IRequestHandler<SignInCommand, IResult<TokenResponse>>
{
    private readonly ISignInService signInService;
    private readonly IEventQueue eventQueue;
    private readonly ApplicationOptions options;

    public SignInRequestHandler(
        ISignInService signInService,
        IOptions<ApplicationOptions> options,
        IEventQueue eventQueue)
    {
        this.signInService = signInService;
        this.eventQueue = eventQueue;
        this.options = options.Value;
    }

    public async Task<IResult<TokenResponse>> Handle(SignInCommand command, CancellationToken cancellationToken)
    {
        var signIn = await signInService.SignInAsync(command.Email, command.Password, command.RememberMe);

        if (false == signIn.IsSuccess)
        {
            return Fail(signIn);
        }

        if (null != signIn.User && null != signIn.Token)
        {
            await eventQueue.UserSignedInAsync(signIn.User.UserName);

            return Result<TokenResponse>.Success(new TokenResponse
            {
                Token = signIn.Token,
                RefreshToken = signIn.User.RefreshToken
            });
        }

        return Result<TokenResponse>.Fail("");
    }

    private static IResult<TokenResponse> Fail(SignInResult result)
    {
        if (result.IsLockedOut)
        {
            return Result<TokenResponse>.Fail("");
        }

        if (result.IsNotAllowed)
        {
            return Result<TokenResponse>.Fail("");
        }

        return Result<TokenResponse>.Fail("");
    }
}