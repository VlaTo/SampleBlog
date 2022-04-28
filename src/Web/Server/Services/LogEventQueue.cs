using SampleBlog.Core.Application.Services;

namespace SampleBlog.Web.Server.Services;

public sealed class LogEventQueue : IEventQueue
{
    private readonly ILogger<LogEventQueue> logger;

    public LogEventQueue(ILogger<LogEventQueue> logger)
    {
        this.logger = logger;
    }

    public Task UserSignedInAsync(string userId)
    {
        logger.LogDebug($"User: {userId} were logged in");
        return Task.CompletedTask;
    }
}