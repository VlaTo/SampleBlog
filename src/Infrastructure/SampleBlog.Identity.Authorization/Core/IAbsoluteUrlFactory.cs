using Microsoft.AspNetCore.Http;

namespace SampleBlog.Identity.Authorization.Core;

internal interface IAbsoluteUrlFactory
{
    string GetAbsoluteUrl(string path);

    string GetAbsoluteUrl(HttpContext context, string path);
}