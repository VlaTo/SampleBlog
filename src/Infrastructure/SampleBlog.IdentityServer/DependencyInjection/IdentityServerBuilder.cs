using Microsoft.Extensions.DependencyInjection;

namespace SampleBlog.IdentityServer.DependencyInjection;

public sealed class IdentityServerBuilder : IIdentityServerBuilder
{
    public IServiceCollection Services
    {
        get;
    }

    public IdentityServerBuilder(IServiceCollection services)
    {
        Services = services;
    }
}