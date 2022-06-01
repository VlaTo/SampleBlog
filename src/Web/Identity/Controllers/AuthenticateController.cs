using MediatR;
using Microsoft.AspNetCore.Mvc;
using SampleBlog.Web.Identity.ViewModels;
using SignInCommand = SampleBlog.Web.Identity.Core.Features.Commands.SignIn.SignInCommand;

namespace SampleBlog.Web.Identity.Controllers
{
    [Route("[controller]")]
    public sealed class AuthenticateController : Controller
    {
        private readonly IMediator mediator;

        public AuthenticateController(IMediator mediator)
        {
            this.mediator = mediator;
        }

        // GET: /authenticate/login
        [HttpGet("login")]
        public IActionResult SignIn()
        {
            var signIn = new SignInModel();
            return View("SignIn", signIn);
        }

        // POST: /authenticate/login
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
                    if (string.IsNullOrEmpty(signIn.RedirectUrl))
                    {
                        return Redirect("~/");
                    }

                    return Redirect(signIn.RedirectUrl);
                }

                ModelState.AddModelError("error_general", "General error");
            }

            return View("SignIn", signIn);
        }

        // GET: /authenticate/reset
        [HttpGet("reset")]
        public IActionResult ResetPassword()
        {
            var passwordModel = new ResetPasswordModel();
            return View("ResetPassword", passwordModel);
        }
    }
}
