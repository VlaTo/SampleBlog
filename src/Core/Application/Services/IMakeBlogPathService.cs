namespace SampleBlog.Core.Application.Services;

public interface IMakeBlogPathService
{
    ValueTask<string> BuildBlogPathAsync(string blogId);
}