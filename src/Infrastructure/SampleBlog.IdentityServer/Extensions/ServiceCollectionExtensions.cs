using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SampleBlog.IdentityServer.DependencyInjection;
using SampleBlog.IdentityServer.DependencyInjection.Extensions;
using SampleBlog.IdentityServer.DependencyInjection.Options;

namespace SampleBlog.IdentityServer.Extensions;

public static class ServiceCollectionExtensions
{
    public static IIdentityServerBuilder AddIdentityServerCore(this IServiceCollection services)
    {
        return new IdentityServerBuilder(services);
    }

    public static IIdentityServerBuilder AddIdentityServer(this IServiceCollection services)
    {
        var builder = services.AddIdentityServerCore();

        builder
            .AddRequiredPlatformServices()
            .AddCookieAuthentication()
            .AddCoreServices()
            .AddKeyManagement()
            .AddDefaultEndpoints()
            .AddResponseGenerators()
            .AddDefaultSecretParsers()
            .AddDefaultSecretValidators()
            ;

        return builder;
    }

    public static IIdentityServerBuilder AddIdentityServer(this IServiceCollection services, Action<IdentityServerOptions> setupAction)
    {
        services.Configure(setupAction);
        return services.AddIdentityServer();
    }

    public static IIdentityServerBuilder AddIdentityServer(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<IdentityServerOptions>(configuration);
        return services.AddIdentityServer();
    }
}