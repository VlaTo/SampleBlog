using SampleBlog.Core.Domain.Entities;

namespace SampleBlog.Core.Domain.Services;

public interface IBlogService
{
    Task<IBlog?> GetBlogAsync(long blogId, CancellationToken cancellationToken = default);
}