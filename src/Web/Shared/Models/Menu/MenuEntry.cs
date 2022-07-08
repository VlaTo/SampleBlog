using System.Text.Json.Serialization;

namespace SampleBlog.Web.Shared.Models.Menu;

[Serializable]
public class MenuEntry
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
    public DishEntry[] Dishes
    {
        get;
        set;
    }

    public MenuEntry()
    {
        Dishes = Array.Empty<DishEntry>();
    }
}