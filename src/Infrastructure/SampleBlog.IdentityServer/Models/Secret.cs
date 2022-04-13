namespace SampleBlog.IdentityServer.Models;

/// <summary>
/// Models a client secret with identifier and expiration
/// </summary>
public sealed class Secret : IEquatable<Secret>
{
    /// <summary>
    /// Gets or sets the description.
    /// </summary>
    /// <value>
    /// The description.
    /// </value>
    public string Description
    {
        get;
    }

    /// <summary>
    /// Gets or sets the value.
    /// </summary>
    /// <value>
    /// The value.
    /// </value>
    public string Value
    {
        get;
    }

    /// <summary>
    /// Gets or sets the expiration.
    /// </summary>
    /// <value>
    /// The expiration.
    /// </value>
    public DateTime? Expiration
    {
        get;
    }

    /// <summary>
    /// Gets or sets the type of the client secret.
    /// </summary>
    /// <value>
    /// The type of the client secret.
    /// </value>
    public string Type
    {
        get;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Secret"/> class.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="expiration">The expiration.</param>
    public Secret(string value, DateTime? expiration = null)
        : this(value, String.Empty, expiration)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Secret" /> class.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="description">The description.</param>
    /// <param name="expiration">The expiration.</param>
    public Secret(string value, string description, DateTime? expiration = null)
    {
        Value = value;
        Description = description;
        Expiration = expiration;
        Type = IdentityServerConstants.SecretTypes.SharedSecret;
    }

    /// <summary>
    /// Returns a hash code for this instance.
    /// </summary>
    /// <returns>
    /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
    /// </returns>
    public override int GetHashCode()
    {
        return HashCode.Combine(Value, Type, Description, Expiration);
    }

    /// <summary>
    /// Determines whether the specified <see cref="System.Object" />, is equal to this instance.
    /// </summary>
    /// <param name="obj">The <see cref="System.Object" /> to compare with this instance.</param>
    /// <returns>
    ///   <c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.
    /// </returns>
    public override bool Equals(object? obj)
    {
        return ReferenceEquals(this, obj) || obj is Secret other && Equals(other);
    }

    public bool Equals(Secret? other)
    {
        if (ReferenceEquals(null, other))
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return String.Equals(Type, other.Type) && String.Equals(Value, other.Value);
    }

    public void Deconstruct(out string value, out string type, out string description, out DateTime? expiration)
    {
        value = Value;
        type = Type;
        description = Description;
        expiration = Expiration;
    }

    public void Deconstruct(out string value, out string type)
    {
        value = Value;
        type = Type;
    }
}