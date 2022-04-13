using SampleBlog.Core.Domain.Entities;
using SampleBlog.Core.Domain.Services;

namespace SampleBlog.Infrastructure.Services;

public sealed class BlogService : IBlogService
{
    public BlogService()
    {
    }

    public Task<Blog?> GetBlogAsync(long blogId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult<Blog?>(new Blog(blogId));
    }
}