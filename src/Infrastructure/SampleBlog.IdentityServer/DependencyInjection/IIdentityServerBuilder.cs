using Microsoft.Extensions.DependencyInjection;

namespace SampleBlog.IdentityServer.DependencyInjection;

public interface IIdentityServerBuilder
{
    IServiceCollection Services
    {
        get;
    }
}