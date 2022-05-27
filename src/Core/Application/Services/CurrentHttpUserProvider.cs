using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace SampleBlog.Core.Application.Services;

public sealed class CurrentHttpUserProvider : ICurrentUserProvider
{
    private readonly IHttpContextAccessor httpContextAccessor;

    public string? CurrentUserId
    {
        get
        {
            var currentUserId = httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
            return currentUserId;
        }
    }

    public CurrentHttpUserProvider(IHttpContextAccessor httpContextAccessor)
    {
        this.httpContextAccessor = httpContextAccessor;
    }
}