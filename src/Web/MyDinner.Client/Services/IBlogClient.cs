namespace SampleBlog.Web.Application.MyDinner.Client.Services;

public interface IBlogClient
{
    Task GetBlogAsync(string blogId, CancellationToken cancellationToken = default);
}