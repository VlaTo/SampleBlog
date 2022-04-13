using SampleBlog.Core.Domain.Entities;

namespace SampleBlog.Core.Domain.Services;

public interface IBlogService
{
    Task<Blog?> GetBlogAsync(long blogId, CancellationToken cancellationToken = default);
}