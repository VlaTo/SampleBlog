using System.Text.Json.Serialization;

namespace SampleBlog.Web.Shared.Models.Menu;

[Serializable]
public class ProductGroupEntry : IEquatable<ProductGroupEntry>
{
    [JsonPropertyName("name")]
    public string? Name
    {
        get;
        set;
    }

    public bool Equals(ProductGroupEntry? other)
    {
        if (ReferenceEquals(null, other))
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return String.Equals(Name, other.Name);
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj))
        {
            return false;
        }

        if (ReferenceEquals(this, obj))
        {
            return true;
        }

        if (obj.GetType() != GetType())
        {
            return false;
        }

        return Equals((ProductGroupEntry)obj);
    }

    public override int GetHashCode()
    {
        //return HashCode.Combine(Name);
        return Name.GetHashCode();
    }
}