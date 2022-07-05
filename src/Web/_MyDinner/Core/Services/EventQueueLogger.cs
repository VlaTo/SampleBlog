using SampleBlog.Core.Application.Services;

namespace SampleBlog.Web.MyDinner.Core.Services;

internal sealed class EventQueueLogger : IEventQueue
{
    private readonly ILogger logger;

    public EventQueueLogger(ILogger<EventQueueLogger> logger)
    {
        this.logger = logger;
    }

    public Task UserSignedInAsync(string userId)
    {
        logger.LogDebug("The User: {0} was logged in", userId);
        return Task.CompletedTask;
    }
}