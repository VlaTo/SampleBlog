using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using SampleBlog.Core.Application.Configuration;
using SampleBlog.Core.Application.Responses.Identity;
using SampleBlog.Core.Application.Services;
using SampleBlog.Shared;

namespace SampleBlog.Core.Application.Features.Commands.Login;

public sealed class SignInRequestHandler : IRequestHandler<SignInCommand, IResult<TokenResponse>>
{
    private readonly ISignInService signInService;
    private readonly ICurrentUserProvider currentUserProvider;
    private readonly IEventQueueProvider queueProvider;
    private readonly ApplicationOptions options;

    public SignInRequestHandler(
        ISignInService signInService,
        IOptions<ApplicationOptions> options,
        ICurrentUserProvider currentUserProvider,
        IEventQueueProvider queueProvider)
    {
        this.signInService = signInService;
        this.currentUserProvider = currentUserProvider;
        this.queueProvider = queueProvider;
        this.options = options.Value;
    }

    public async Task<IResult<TokenResponse>> Handle(SignInCommand command, CancellationToken cancellationToken)
    {
        var user = await signInService.FindUserAsync(command.Request.Email);

        if (null == user)
        {
            return Result<TokenResponse>.Fail("");
        }

        if (false == user.EmailConfirmed)
        {
            ;
        }

        var result = await signInService.ValidateCredentials(user, command.Request.Password);

        if (false == result.Succeeded)
        {
            return Result<TokenResponse>.Fail("");
        }

        AuthenticationProperties? properties = null;

        if (options.Authentication.AllowRememberMe)
        {
            properties = new AuthenticationProperties
            {
                IsPersistent = true,
                IssuedUtc = DateTimeOffset.UtcNow,
                ExpiresUtc = DateTimeOffset.UtcNow + options.Authentication.RememberMeSignInDuration
            };
        }

        result = await signInService.SignInAsync(user, properties, "");

        if (result.Succeeded)
        {
            var queue = await queueProvider.GetQueueAsync();
            var currentUserId = currentUserProvider.CurrentUserId;

            if (null != currentUserId)
            {
                await queue.UserSignedInAsync(currentUserId);
            }

            return Result<TokenResponse>.Success(new TokenResponse
            {

            });
        }

        return Result<TokenResponse>.Fail("");
    }
}