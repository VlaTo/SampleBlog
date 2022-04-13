namespace SampleBlog.Core.Application.Services;

public interface IEventQueueProvider
{
    Task<IEventQueue> GetQueueAsync();
}