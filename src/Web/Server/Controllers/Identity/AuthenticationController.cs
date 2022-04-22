using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SampleBlog.Core.Application.Features.Commands.Login;
using SampleBlog.Web.Server.ViewModels;

namespace SampleBlog.Web.Server.Controllers.Identity
{
    [Route("[controller]")]
    public class AuthenticationController : Controller
    {
        private readonly IMediator mediator;

        public AuthenticationController(IMediator mediator)
        {
            this.mediator = mediator;
        }

        [AllowAnonymous]
        [HttpGet("login")]
        public IActionResult SignIn()
        {
            var signIn = new SignInModel();
            return View("SignIn", signIn);
        }

        [AllowAnonymous]
        [Consumes("application/x-www-form-urlencoded")]
        [HttpPost("login")]
        public async Task<IActionResult> SignIn([FromForm] SignInModel signIn)
        {
            if (ModelState.IsValid)
            {
                var command = new SignInCommand(signIn.Email, signIn.Password, signIn.RememberMe);
                var result = await mediator.Send(command, HttpContext.RequestAborted);

                if (result is { Succeeded: true })
                {
                    if (String.IsNullOrEmpty(signIn.RedirectUrl))
                    {
                        return Redirect("~/");
                    }

                    return Redirect(signIn.RedirectUrl);
                }

                ModelState.AddModelError("error_general", "General error");
            }

            return View("SignIn", signIn);
        }
    }
}
