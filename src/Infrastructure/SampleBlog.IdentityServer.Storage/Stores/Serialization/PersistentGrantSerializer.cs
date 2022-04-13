using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.DataProtection;

namespace SampleBlog.IdentityServer.Storage.Stores.Serialization;

/// <summary>
/// Options for how persisted grants are persisted.
/// </summary>
public class PersistentGrantOptions
{
    /// <summary>
    /// Data protect the persisted grants "data" column.
    /// </summary>
    public bool DataProtectData
    {
        get;
        set;
    }

    public PersistentGrantOptions()
    {
        DataProtectData = true;
    }
}

/// <summary>
/// JSON-based persisted grant serializer
/// </summary>
/// <seealso cref="IPersistentGrantSerializer" />
public class PersistentGrantSerializer : IPersistentGrantSerializer
{
    private static readonly JsonSerializerOptions Settings;

    private readonly PersistentGrantOptions? options;
    private readonly IDataProtector? provider;

    private bool ShouldDataProtect => options is { DataProtectData: true };

    static PersistentGrantSerializer()
    {
        Settings = new JsonSerializerOptions
        {
            IgnoreReadOnlyFields = true,
            IgnoreReadOnlyProperties = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        Settings.Converters.Add(new ClaimConverter());
        Settings.Converters.Add(new ClaimsPrincipalConverter());
    }

    /// <summary>
    /// Ctor.
    /// </summary>
    /// <param name="options"></param>
    /// <param name="dataProtectionProvider"></param>
    public PersistentGrantSerializer(
        PersistentGrantOptions? options = null,
        IDataProtectionProvider? dataProtectionProvider = null)
    {
        this.options = options;
        provider = dataProtectionProvider?.CreateProtector(nameof(PersistentGrantSerializer));
    }

    /// <summary>
    /// Serializes the specified value.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="value">The value.</param>
    /// <returns></returns>
    public string Serialize<T>(T value)
    {
        var payload = JsonSerializer.Serialize(value, Settings);

        if (ShouldDataProtect && null != provider)
        {
            payload = provider.Protect(payload);
        }

        var data = new PersistentGrantDataContainer
        {
            Version = 1,
            DataProtected = ShouldDataProtect,
            Payload = payload
        };

        return JsonSerializer.Serialize(data, Settings);
    }

    /// <summary>
    /// Deserializes the specified string.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="json">The json.</param>
    /// <returns></returns>
    public T Deserialize<T>(string json)
    {
        var container = JsonSerializer.Deserialize<PersistentGrantDataContainer>(json, Settings);

        if (null != container)
        {
            if (0 == container.Version)
            {
                return JsonSerializer.Deserialize<T>(json, Settings);
            }

            if (1 == container.Version)
            {
                var payload = container.Payload;

                if (container.DataProtected)
                {
                    if (null == provider)
                    {
                        throw new Exception("No IDataProtectionProvider configured.");
                    }

                    payload = provider.Unprotect(container.Payload);
                }

                return JsonSerializer.Deserialize<T>(payload, Settings);
            }
        }

        throw new Exception($"Invalid version in persisted grant data: '{container.Version}'.");
    }

    #region PersistentGrantDataContainer

    internal class PersistentGrantDataContainer
    {
        public int Version
        {
            get;
            set;
        }

        public bool DataProtected
        {
            get;
            set;
        }

        public string Payload
        {
            get;
            set;
        }
    }

    #endregion
}
