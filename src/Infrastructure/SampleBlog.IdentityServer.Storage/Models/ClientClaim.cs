using System.Security.Claims;

namespace SampleBlog.IdentityServer.Storage.Models;

/// <summary>
/// A client claim
/// </summary>
public sealed class ClientClaim : IEquatable<ClientClaim>
{
    /// <summary>
    /// The claim type
    /// </summary>
    public string Type
    {
        get;
    }

    /// <summary>
    /// The claim value
    /// </summary>
    public string Value
    {
        get;
    }

    /// <summary>
    /// The claim value type
    /// </summary>
    public string ValueType
    {
        get;
    }

    /// <summary>
    /// ctor
    /// </summary>
    /// <param name="type"></param>
    /// <param name="value"></param>
    public ClientClaim(string type, string value)
        : this(type, value, ClaimValueTypes.String)
    {
    }

    /// <summary>
    /// ctor
    /// </summary>
    /// <param name="type"></param>
    /// <param name="value"></param>
    /// <param name="valueType"></param>
    public ClientClaim(string type, string value, string valueType)
    {
        Type = type;
        Value = value;
        ValueType = valueType;
    }

    public bool Equals(ClientClaim? other)
    {
        if (ReferenceEquals(null, other))
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return String.Equals(Type, other.Type)
               && String.Equals(Value, other.Value)
               && String.Equals(ValueType, other.ValueType);
    }

    public override bool Equals(object? obj)
    {
        return ReferenceEquals(this, obj) || obj is ClientClaim other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Type, Value, ValueType);
    }

    public void Deconstruct(out string type, out string value, out string valueType)
    {
        type = Type;
        value = Value;
        valueType = ValueType;
    }
}