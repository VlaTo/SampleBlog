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

    [JsonPropertyName("enabled")]
    public bool IsEnabled
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

    [JsonPropertyName("outcome")]
    public OutcomeEntry Outcome
    {
        get;
        set;
    }

    [JsonPropertyName("calories")]
    public float Calories
    {
        get;
        set;
    }

    [JsonPropertyName("price")]
    public decimal Price
    {
        get;
        set;
    }

    [JsonPropertyName("group-name")]
    public string? GroupName
    {
        get;
        set;
    }
}