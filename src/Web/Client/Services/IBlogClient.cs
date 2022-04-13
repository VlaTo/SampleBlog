namespace SampleBlog.Web.Client.Services;

public interface IBlogClient
{
    Task GetBlogAsync(string blogId, CancellationToken cancellationToken = default);
}