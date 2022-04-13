using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SampleBlog.MessageBroker.Configuration;
using SampleBlog.MessageBroker.Services;

var host = new HostBuilder()
    .ConfigureHostConfiguration(configuration =>
    {
        configuration
            .AddEnvironmentVariables()
            .AddCommandLine(args);
    })
    .ConfigureAppConfiguration((context,configuration) =>
    {
        configuration
            .SetBasePath(context.HostingEnvironment.ContentRootPath)
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile($"appsettings.{context.HostingEnvironment.EnvironmentName}.json", optional: true)
            .AddCommandLine(args);
    })
    .ConfigureServices((context, services) =>
    {
        services.AddLogging(options =>
        {
            options
                .AddConfiguration(context.Configuration.GetSection("Logging"))
                .AddDebug()
                .AddConsole();
        });

        services
            .AddOptions<MqttServerOptions>()
            .Bind(context.Configuration.GetSection("Mqtt.Server"))
            .Configure((MqttServerOptions options, IHostEnvironment environment) =>
            {
                ;
            })
            .Validate((MqttServerOptions options, IHostEnvironment environment) =>
            {
                return true;
            });

        if (context.HostingEnvironment.IsDevelopment())
        {
            ;
        }

        services.AddHostedService<MqttServerHostedService>();
    })
    .UseConsoleLifetime()
    .Build();

await host.RunAsync();
