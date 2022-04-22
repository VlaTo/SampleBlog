using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SampleBlog.Core.Application.Configuration;
using SampleBlog.Core.Application.Models.Identity;
using SampleBlog.Core.Application.Responses.Identity;
using SampleBlog.Core.Application.Services;
using SampleBlog.Shared;
using SignInResult = SampleBlog.Core.Application.Services.SignInResult;

namespace SampleBlog.Core.Application.Features.Commands.Login;

public sealed class SignInRequestHandler : IRequestHandler<SignInCommand, IResult<TokenResponse>>
{
    private readonly ISignInService signInService;
    private readonly IEventQueueProvider eventQueueProvider;
    private readonly ApplicationOptions options;

    public SignInRequestHandler(
        ISignInService signInService,
        IOptions<ApplicationOptions> options,
        IEventQueueProvider eventQueueProvider)
    {
        this.signInService = signInService;
        this.eventQueueProvider = eventQueueProvider;
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
            var eventQueue = await eventQueueProvider.GetQueueAsync();

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