using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace SampleBlog.MessageBroker.Services;

public abstract class BaseHostedService : IHostedService, IDisposable
{
    private readonly CancellationTokenSource cts;
    private Task? execution;

    public ILogger Logger
    {
        get;
    }

    protected BaseHostedService(ILogger logger)
    {
        cts = new CancellationTokenSource();
        execution = null;
        Logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        execution = Task.Factory.StartNew(() => ExecuteAsync(cts.Token), TaskCreationOptions.LongRunning);

        if (execution.IsCompleted)
        {
            return execution;
        }

        return Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        if (null == execution)
        {
            return;
        }

        try
        {
            cts.Cancel();
        }
        finally
        {
            await Task.WhenAny(execution, Task.Delay(Timeout.Infinite, cts.Token));
        }
    }

    public abstract Task ExecuteAsync(CancellationToken cancellationToken);

    public void Dispose()
    {
        cts.Cancel();
    }
}