using System.Security.Claims;
using SampleBlog.Core.Application.Services;

namespace SampleBlog.Web.Server.Services;

internal sealed class CurrentHttpUserProvider : ICurrentUserProvider
{
    private readonly IHttpContextAccessor httpContextAccessor;
    private bool fetched;
    private string? currentUserId;

    public string? CurrentUserId
    {
        get
        {
            if (false == fetched)
            {
                currentUserId = httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
                fetched = true;
            }

            return currentUserId;
        }
    }

    public CurrentHttpUserProvider(IHttpContextAccessor httpContextAccessor)
    {
        this.httpContextAccessor = httpContextAccessor;
        //CurrentUserId = httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
    }
}