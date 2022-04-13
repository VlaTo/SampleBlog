using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SampleBlog.Core.Application.Features.Commands.Login;
using SampleBlog.Core.Application.Requests.Identity;

namespace SampleBlog.Web.Server.Controllers.Identity
{
    [Route("api/identity/token")]
    [ApiController]
    public class TokenController : ControllerBase
    {
        private readonly IMediator mediator;

        public TokenController(IMediator mediator)
        {
            this.mediator = mediator;
        }

        [AllowAnonymous]
        [Consumes("application/json","application/x-www-form-urlencoded")]
        [HttpPost("")]
        public async Task<IActionResult> SignIn([FromBody] SignInRequest request)
        {
            if (ModelState.IsValid)
            {
                var command = new SignInCommand(request);
                var result = await mediator.Send(command, HttpContext.RequestAborted);

                if (result is { Succeeded: true })
                {
                    var claims = new[]
                    {
                        new Claim(ClaimTypes.NameIdentifier, "test name"),
                        new Claim(ClaimTypes.Actor, "testUser"),
                        new Claim(ClaimTypes.Email, request.Email),
                        new Claim(ClaimTypes.Anonymous, "true", ClaimValueTypes.Boolean),
                    };

                    var identity = new ClaimsIdentity(claims, "Bearer");
                    var principal = new ClaimsPrincipal(identity);

                    return SignIn(principal);
                }
            }
            else
            {
                return BadRequest();
            }

            return Unauthorized();
        }
    }
}
