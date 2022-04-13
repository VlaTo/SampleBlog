using SampleBlog.Web.Server.Configuration;

namespace SampleBlog.Web.Server.Extensions;

internal static class WebApplicationBuilderExtensions
{
    internal static ServerOptions GetServerOptions(this WebApplicationBuilder builder)
    {
        var applicationSettingsConfiguration = builder.Configuration.GetSection(nameof(ServerOptions));

        builder.Services.Configure<ServerOptions>(applicationSettingsConfiguration);
        
        return applicationSettingsConfiguration.Get<ServerOptions>();
    }
}