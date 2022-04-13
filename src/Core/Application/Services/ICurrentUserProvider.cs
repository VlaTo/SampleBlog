namespace SampleBlog.Core.Application.Services;

public interface ICurrentUserProvider
{
    string? CurrentUserId
    {
        get;
    }
}