using System.Security.Claims;
using SampleBlog.Core.Application.Services;

namespace SampleBlog.Web.Server.Services;

internal sealed class CurrentHttpUserProvider : ICurrentUserProvider
{
    public string? CurrentUserId { get; }

    public CurrentHttpUserProvider(IHttpContextAccessor accessor)
    {
        CurrentUserId = accessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
    }
}