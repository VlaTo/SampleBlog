using System.Text.Json.Serialization;

namespace SampleBlog.Web.Shared.Models.Menu;

[Serializable]
public class Menu
{
    [JsonPropertyName("date")]
    public DateTime Date
    {
        get;
        set;
    }

    [JsonPropertyName("open")]
    public bool IsOpen
    {
        get;
        set;
    }

    [JsonPropertyName("dishes")]
    public Dish[] Dishes
    {
        get;
        set;
    }

    public Menu()
    {
        Dishes = Array.Empty<Dish>();
    }
}