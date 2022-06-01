using HashidsNet;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SampleBlog.Core.Application.Services;
using SampleBlog.Shared.Contracts.Permissions;
using SampleBlog.Web.APi.Blog.Configuration;
using AddBlogCommand = SampleBlog.Web.APi.Blog.Features.Commands.AddBlog.AddBlogCommand;
using GetBlogQuery = SampleBlog.Web.APi.Blog.Features.Queries.GetBlog.GetBlogQuery;

namespace SampleBlog.Web.APi.Blog.Controllers.v1
{
    //[AllowAnonymous]
    [ApiVersion("1")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    public class BlogController : ControllerBase
    {
        private readonly IMediator mediator;
        private readonly ICurrentUserProvider currentUserProvider;
        private readonly BlogOptions options;

        public BlogController(
            IMediator mediator,
            ICurrentUserProvider currentUserProvider,
            IOptions<BlogOptions> options)
        {
            this.mediator = mediator;
            this.currentUserProvider = currentUserProvider;
            this.options = options.Value;
        }

        [HttpGet("{hid:required}")]
        public async Task<IActionResult> Get([FromRoute] string hid)
        {
            // Vd4n7zrM73YN -- blogId: 101
            var hash = new Hashids(salt: options.HashId.Salt, minHashLength: options.HashId.MinHashLength);
            var numbers = hash.DecodeLong(hid);

            if (numbers is { Length: 1 })
            {
                var blogId = numbers[0];
                var query = new GetBlogQuery(blogId, currentUserProvider.CurrentUserId);
                
                var result = await mediator.Send(query, HttpContext.RequestAborted);

                if (result.Succeeded)
                {
                    return Ok(result.Data);
                }
            }
            else
            {
                return BadRequest(new
                {
                    BlogId = hid
                });
            }

            return NotFound();
        }

        [Authorize(Roles = Permissions.Blog.Create)]
        [HttpPost]
        public async Task<IActionResult> Add([FromBody] string content, [FromServices] IMakeBlogPathService pathService)
        {
            if (false == ModelState.IsValid)
            {
                return ValidationProblem();
            }

            var command = new AddBlogCommand(currentUserProvider.CurrentUserId, content);

            var result = await mediator.Send(command, HttpContext.RequestAborted);

            if (result.Succeeded)
            {
                var blogPath = await pathService.BuildBlogPathAsync(result.Data);
                return Created(blogPath, null);
            }

            return BadRequest();
        }
    }
}