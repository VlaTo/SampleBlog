using System.Text.Json.Serialization;
using SampleBlog.Core.Domain.Entities;

namespace SampleBlog.Web.Shared.Models.Menu;

[Serializable]
public class Outcome
{
    [JsonPropertyName("amount")]
    public float Amount
    {
        get;
        set;
    }

    [JsonPropertyName("units")]
    public Units Units
    {
        get;
        set;
    }

    [JsonPropertyName("custom_units")]
    public string? CustomUnits
    {
        get;
        set;
    }
}