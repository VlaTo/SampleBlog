using System.Text.Json.Serialization;

namespace SampleBlog.Web.Shared.Models.Menu;

[Serializable]
public class OutcomeEntry
{
    [JsonPropertyName("amount")]
    public float Amount
    {
        get;
        set;
    }

    [JsonPropertyName("units")]
    public string Units
    {
        get;
        set;
    }
}