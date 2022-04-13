namespace SampleBlog.IdentityServer.Core;

internal class Decorator<TService>
{
    public TService Instance
    {
        get;
    }

    public Decorator(TService instance)
    {
        Instance = instance;
    }

    public static Decorator<TService> CreateDisposable(TService instance) => new DisposableDecorator(instance);

    private class DisposableDecorator : Decorator<TService>, IDisposable
    {
        public DisposableDecorator(TService instance)
            : base(instance)
        {
        }

        public void Dispose() => (Instance as IDisposable)?.Dispose();
    }
}

internal class Decorator<TService, TImpl> : Decorator<TService>
    where TImpl : class, TService
{
    public Decorator(TImpl instance)
        : base(instance)
    {
    }
}