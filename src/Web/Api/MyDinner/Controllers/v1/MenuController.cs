using HashidsNet;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SampleBlog.Core.Application.Services;
using SampleBlog.Web.APi.MyDinner.Configuration;
using SampleBlog.Web.APi.MyDinner.Features.Queries.GetMenu;
using SampleBlog.Web.Shared.Models.Menu;
using System.Diagnostics;

namespace SampleBlog.Web.APi.Blog.Controllers.v1
{
    //[AllowAnonymous]
    [ApiVersion("1")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    public class MenuController : ControllerBase
    {
        private readonly IMediator mediator;
        private readonly ICurrentUserProvider currentUserProvider;
        private readonly MyDinnerOptions options;

        public MenuController(
            IMediator mediator,
            ICurrentUserProvider currentUserProvider,
            IOptions<MyDinnerOptions> options)
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
            
            {
                var temp0 = DateOnly.FromDateTime(DateTime.UtcNow);
                var temp1 = temp0.ToDateTime(new TimeOnly());
                var temp2 = temp1.ToBinary();
                var temp3 = hash.EncodeLong(temp2);

                Debug.WriteLine($"Current date hash: {temp3}");
            }

            if (hash.TryDecodeSingleLong(hid, out var value))
            {
                var dateTime = DateTime.FromBinary(value);
                var query = new GetMenuQuery(dateTime);
                
                var result = await mediator.Send(query, HttpContext.RequestAborted);

                if (result.Succeeded)
                {
                    var menu = new MenuEntry
                    {
                        Date = result.Data.Date,
                        Dishes = new []
                        {
                            new DishEntry
                            {
                                Order = 2,
                                ProductName = "Sample Product #2"
                            },
                            new DishEntry
                            {
                                Order = 1,
                                ProductName = "Sample Product #1"
                            },
                            new DishEntry
                            {
                                Order = 3,
                                ProductName = "Sample Product #3"
                            }
                        }
                    };

                    return Ok(menu);
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

        /*[Authorize(Roles = Permissions.Blog.Create)]
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
        }*/
    }
}