using System.Text.Json;

namespace SampleBlog.IdentityServer.Services.KeyManagement;

internal static class KeySerializer
{
    private static JsonSerializerOptions settings = new JsonSerializerOptions
    {
        IncludeFields = true
    };

    public static string Serialize<T>(T item)
    {
        return JsonSerializer.Serialize(item, item.GetType(), settings);
    }

    public static T? Deserialize<T>(string json)
    {
        return JsonSerializer.Deserialize<T>(json, settings);
    }
}