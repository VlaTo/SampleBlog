using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SampleBlog.Core.Application.Configuration;
using SampleBlog.Core.Application.Services;

namespace SampleBlog.Core.Application.Extensions;

public static class ServiceCollectionExtensions
{
    internal const string SectionName = "Application";

    public static IServiceCollection AddApplicationOptions(this IServiceCollection services)
    {
        services
            .AddOptions<ApplicationOptions>(SectionName)
            .Configure(configuration =>
            {
                ;
            })
            .Validate(configuration =>
            {
                return true;
            });

        return services;
    }

    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.TryAddTransient<ICurrentUserProvider, CurrentHttpUserProvider>();
        //services.TryAddSingleton<IEventQueue, LogEventQueue>();

        return services;
    }

    public static ApplicationOptions GetApplicationOptions(this IServiceCollection services, IConfiguration configuration)
    {
        var configurationSection = configuration.GetSection(SectionName);
        services.Configure<ApplicationOptions>(configurationSection);
        return configurationSection.Get<ApplicationOptions>();
    }
}