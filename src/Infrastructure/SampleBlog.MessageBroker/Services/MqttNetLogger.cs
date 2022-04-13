using Microsoft.Extensions.Logging;
using MQTTnet.Diagnostics.Logger;

namespace SampleBlog.MessageBroker.Services;

internal sealed class MqttNetLogger : IMqttNetLogger
{
    private readonly ILogger logger;

    public bool IsEnabled
    {
        get;
        set;
    }

    public MqttNetLogger(ILogger logger)
    {
        this.logger = logger;
    }

    public void Publish(MqttNetLogLevel logLevel, string source, string message, object[] parameters, Exception exception)
    {
        switch (logLevel)
        {
            case MqttNetLogLevel.Verbose:
            case MqttNetLogLevel.Info:
            case MqttNetLogLevel.Warning:
            case MqttNetLogLevel.Error:
            {
                if (null != exception)
                {
                    logger.LogDebug(exception, message, parameters);
                }
                else
                {
                    logger.LogDebug(message, parameters);
                }

                break;
            }
        }
    }
}