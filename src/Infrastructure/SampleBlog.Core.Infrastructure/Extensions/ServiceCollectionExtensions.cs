using Microsoft.Extensions.DependencyInjection;
using SampleBlog.Core.Domain.Services;
using SampleBlog.Infrastructure.Services;

namespace SampleBlog.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services
            .AddTransient<IBlogService, BlogService>();

        return services;
    }
}