using MediatR;
using Microsoft.AspNetCore.Mvc;
using SampleBlog.Web.Identity.Core;
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
        public IActionResult SignIn([FromQuery(Name = "returnUrl")] string returnUrl)
        {
            ViewData[Constants.ReturnUrl] = returnUrl;

            var signIn = new SignInModel
            {
                Email = "superuser@sampleblog.net",
                Password = "a1B2c.3",
                RememberMe = false
            };

            return View("SignIn", signIn);
        }

        // POST: /authenticate/login
        [Consumes("application/x-www-form-urlencoded")]
        [HttpPost("login")]
        public async Task<IActionResult> SignIn([FromForm] SignInModel signIn, [FromForm(Name = "returnUrl")] string returnUrl)
        {
            if (ModelState.IsValid)
            {
                var command = new SignInCommand(signIn.Email, signIn.Password, signIn.RememberMe);
                var result = await mediator.Send(command, HttpContext.RequestAborted);

                if (result is { Succeeded: true })
                {
                    return RedirectToUrl(returnUrl);
                }

                ModelState.AddModelError("error_general", "General error");
            }

            ViewData[Constants.ReturnUrl] = returnUrl;

            return View("SignIn", signIn);
        }

        // GET: /authenticate/reset
        [HttpGet("reset")]
        public IActionResult ResetPassword()
        {
            var passwordModel = new ResetPasswordModel();
            return View("ResetPassword", passwordModel);
        }

        private IActionResult RedirectToUrl(string url)
        {
            if (false == Url.IsLocalUrl(url))
            {
                return Redirect(url);
            }

            return RedirectToAction(nameof(SignIn));
        }
    }
}
