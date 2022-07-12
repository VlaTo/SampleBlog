using System.Text.Json.Serialization;

namespace SampleBlog.Web.Shared.Models.Menu;

[Serializable]
public sealed class Dish
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
    public Product Product
    {
        get;
        set;
    }

    [JsonPropertyName("outcome")]
    public Outcome Outcome
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

    [JsonPropertyName("food-category")]
    public FoodCategory? FoodCategory
    {
        get;
        set;
    }
}