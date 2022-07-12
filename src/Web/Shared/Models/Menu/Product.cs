using System.Text.Json.Serialization;

namespace SampleBlog.Web.Shared.Models.Menu;

[Serializable]
public sealed class Product
{
    [JsonPropertyName("id")]
    public long Id
    {
        get;
        set;
    }

    [JsonPropertyName("name")]
    public string Name
    {
        get;
        set;
    }
}