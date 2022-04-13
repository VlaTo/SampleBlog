using System.Security.Claims;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Publishing;
using SampleBlog.Core.Application.Services;

namespace SampleBlog.Web.Server.Services;

public sealed class MqttEventQueue : IEventQueue
{
    private readonly IMqttClient client;
    
    public MqttEventQueue(IMqttClient client)
    {
        this.client = client;
    }

    public async Task UserSignedInAsync(string userId)
    {
        var builder = new MqttApplicationMessageBuilder();
        var message = builder
            .WithTopic("User/Identity/Signin")
            .WithPayload(userId)
            .Build();

        var result = await client.PublishAsync(message);

        if (MqttClientPublishReasonCode.Success == result.ReasonCode)
        {
            ;
        }
    }
}