using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Connecting;
using MQTTnet.Client.Options;
using SampleBlog.Core.Application.Services;

namespace SampleBlog.Web.Server.Services;

public class MqttEventQueueProvider : IEventQueueProvider
{
    private readonly IMqttFactory factory;
    private MqttEventQueue? queue;

    public MqttEventQueueProvider(IMqttFactory factory)
    {
        this.factory = factory;
        queue = null;
    }

    public async Task<IEventQueue> GetQueueAsync()
    {
        if (null != queue)
        {
            return queue;
        }

        var client = factory.CreateMqttClient();

        if (false == client.IsConnected)
        {
            var options = new MqttClientOptionsBuilder()
                .WithTcpServer("localhost", 5003)
                .Build();

            var result = await client.ConnectAsync(options);

            if (MqttClientConnectResultCode.Success != result.ResultCode)
            {
                throw new Exception();
            }
        }

        queue = new MqttEventQueue(client);

        return queue;
    }
}