using Microsoft.Extensions.DependencyInjection;
using SampleBlog.Infrastructure.Database.Contexts;
using SampleBlog.Infrastructure.Repositories;

namespace SampleBlog.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services
            .AddTransient<BlogContext>()
            .AddTransient<IBlogRepository, BlogRepository>();

        return services;
    }
}