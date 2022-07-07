using System.Text.Json.Serialization;

namespace SampleBlog.Web.Shared.Models.Menu;

[Serializable]
public sealed class DishEntry
{
    [JsonPropertyName("order")]
    public int Order
    {
        get;
        set;
    }

    [JsonPropertyName("product")]
    public string ProductName
    {
        get;
        set;
    }
}