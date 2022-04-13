namespace SampleBlog.Core.Application.Services;

public interface IEventQueue
{
    Task UserSignedInAsync(string userId);
}