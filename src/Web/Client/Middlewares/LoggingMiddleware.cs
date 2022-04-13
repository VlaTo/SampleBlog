﻿using Fluxor;

namespace SampleBlog.Web.Client.Middlewares;

internal sealed class LoggingMiddleware : Middleware
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