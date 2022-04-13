using System.Net;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MQTTnet;
using SampleBlog.MessageBroker.Extensions;
using MqttServerOptions = SampleBlog.MessageBroker.Configuration.MqttServerOptions;

namespace SampleBlog.MessageBroker.Services;

public sealed class MqttServerHostedService : BaseHostedService
{
    private readonly IHostEnvironment environment;
    private readonly IHostApplicationLifetime lifetime;
    private readonly MqttServerOptions options;

    public MqttServerHostedService(
        IOptions<MqttServerOptions> options,
        IHostEnvironment environment,
        IHostApplicationLifetime lifetime,
        ILogger<MqttServerHostedService> logger)
        : base(logger)
    {
        this.environment = environment;
        this.lifetime = lifetime;
        this.options = options.Value;
    }

    public override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        var mqttLogger = new MqttNetLogger(Logger)
        {
            IsEnabled = true
        };

        Logger.LogDebug($"{typeof(MqttServerHostedService)} Executing");
        
        await lifetime.ApplicationStarted.AsTask();

        var factory = new MqttFactory(mqttLogger);
        var optionsBuilder = factory.CreateServerOptionsBuilder();
        var server = factory.CreateMqttServer(mqttLogger);

        optionsBuilder
            .WithDefaultEndpointBoundIPAddress(IPAddress.Loopback)
            .WithDefaultEndpointPort(5003);
        
        try
        {
            var serverOptions = optionsBuilder.Build();

            await Task.WhenAll(server.StartAsync(serverOptions), Task.Delay(Timeout.Infinite, cancellationToken));
            
            lifetime.StopApplication();
        }
        catch
        {
            await server.StopAsync();
        }
        finally
        {
            Logger.LogDebug($"{typeof(MqttServerHostedService)} Done");
        }
    }
}