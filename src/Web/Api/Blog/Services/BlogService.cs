using Microsoft.Extensions.Options;
using SampleBlog.Core.Domain.Entities;
using SampleBlog.Core.Domain.Services;
using SampleBlog.Web.APi.Blog.Configuration;
using SampleBlog.Web.APi.Blog.Models;

namespace SampleBlog.Web.APi.Blog.Services;

internal class BlogService : IBlogService
{
    private readonly BlogOptions options;

    public BlogService(IOptions<BlogOptions> options)
    {
        this.options = options.Value;
    }

    public Task<IBlog?> GetBlogAsync(long blogId, CancellationToken cancellationToken = default)
    {
        var author = new BlogAuthor("2u54yui45")
        {
            IsActive = true,
            UserName = "435kj6h34 j456hk34"
        };
        var blog = new BlogModel(blogId, "Sample Blog", "sample-blog", author);

        return Task.FromResult<IBlog?>(blog);
    }
}