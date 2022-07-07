namespace SampleBlog.Web.Client.Middleware;

internal sealed class LoggingMiddleware : Fluxor.Middleware
{
    public LoggingMiddleware()
    {
    }

    public override bool MayDispatchAction(object action)
    {
        Console.WriteLine($"Dispatching action: {action.GetType().Name}");
        return base.MayDispatchAction(action);
    }
}