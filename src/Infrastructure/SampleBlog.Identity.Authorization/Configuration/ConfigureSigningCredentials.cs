using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SampleBlog.Identity.Authorization.Options;

namespace SampleBlog.Identity.Authorization.Configuration;

internal sealed class ConfigureSigningCredentials: IConfigureOptions<ApiAuthorizationOptions>
{
    private const X509KeyStorageFlags UnsafeEphemeralKeySet = (X509KeyStorageFlags)32;
    private const string DefaultTempKeyRelativePath = "obj/tempkey.json";
    private readonly IConfiguration configuration;
    private readonly ILogger<ConfigureSigningCredentials> logger;

    public ConfigureSigningCredentials(
        IConfiguration configuration,
        ILogger<ConfigureSigningCredentials> logger)
    {
        this.configuration = configuration;
        this.logger = logger;
    }

    public void Configure(ApiAuthorizationOptions options)
    {
        var key = LoadKey();

        if (null != key)
        {
            options.SigningCredential = key;
        }
    }

    public SigningCredentials? LoadKey()
    {
        // We can't know for sure if there was a configuration section explicitly defined.
        // Check if the current configuration has any children and avoid failing if that's the case.
        // This will avoid failing when no configuration has been specified but will still fail if partial data
        // was defined.
        if (!configuration.GetChildren().Any())
        {
            return null;
        }

        var key = new KeyDefinition
        {
            Type = configuration[nameof(KeyDefinition.Type)],
            FilePath = configuration[nameof(KeyDefinition.FilePath)],
            Password = configuration[nameof(KeyDefinition.Password)],
            Name = configuration[nameof(KeyDefinition.Name)],
            StoreLocation = configuration[nameof(KeyDefinition.StoreLocation)],
            StoreName = configuration[nameof(KeyDefinition.StoreName)],
            StorageFlags = configuration[nameof(KeyDefinition.StorageFlags)]
        };

        if (bool.TryParse(configuration[nameof(KeyDefinition.Persisted)], out var value))
        {
            key.Persisted = value;
        }

        switch (key.Type)
        {
            case KeySources.Development:
            {
                var developmentKeyPath = Path.Combine(Directory.GetCurrentDirectory(), key.FilePath ?? DefaultTempKeyRelativePath);
                var createIfMissing = key.Persisted ?? true;

                logger.LogInformation(LoggerEventIds.DevelopmentKeyLoaded, "Loading development key at '{developmentKeyPath}'.", developmentKeyPath);

                var developmentKey = new RsaSecurityKey(SigningKeysLoader.LoadDevelopment(developmentKeyPath, createIfMissing))
                {
                    KeyId = "Development"
                };

                return new SigningCredentials(developmentKey, "RS256");
            }

            case KeySources.File:
            {
                var pfxPath = Path.Combine(Directory.GetCurrentDirectory(), key.FilePath);
                var storageFlags = GetStorageFlags(key);

                logger.LogInformation(
                    LoggerEventIds.CertificateLoadedFromFile,
                    "Loading certificate file at '{CertificatePath}' with storage flags '{CertificateStorageFlags}'.",
                    pfxPath, key.StorageFlags
                );

                return new SigningCredentials(
                    new X509SecurityKey(SigningKeysLoader.LoadFromFile(pfxPath, key.Password, storageFlags)),
                    "RS256"
                );
            }

            case KeySources.Store:
            {
                if (false == Enum.TryParse<StoreLocation>(key.StoreLocation, out var storeLocation))
                {
                    throw new InvalidOperationException($"Invalid certificate store location '{key.StoreLocation}'.");
                }

                logger.LogInformation(
                    LoggerEventIds.CertificateLoadedFromStore,
                    "Loading certificate with subject '{CertificateSubject}' in '{CertificateStoreLocation}\\{CertificateStoreName}'.",
                    key.Name, key.StoreLocation, key.StoreName
                );

                var cert = SigningKeysLoader.LoadFromStoreCert(key.Name, key.StoreName, storeLocation, GetCurrentTime());
                var securityKey = new X509SecurityKey(cert);

                return new SigningCredentials(securityKey, "RS256");
            }

            default:
            {
                throw new InvalidOperationException($"Invalid key type '{key.Type ?? "(null)"}'.");
            }
        }
    }

    internal DateTimeOffset GetCurrentTime() => DateTimeOffset.UtcNow;

    private static X509KeyStorageFlags GetStorageFlags(KeyDefinition key)
    {
        var defaultFlags = OperatingSystem.IsLinux()
            ? UnsafeEphemeralKeySet
            : (OperatingSystem.IsMacOS() ? X509KeyStorageFlags.PersistKeySet : X509KeyStorageFlags.DefaultKeySet);

        if (null == key.StorageFlags)
        {
            return defaultFlags;
        }

        var flagsList = key.StorageFlags.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        if (0 == flagsList.Length)
        {
            return defaultFlags;
        }

        var result = ParseCurrentFlag(flagsList[0]);

        foreach (var flag in flagsList.Skip(1))
        {
            result |= ParseCurrentFlag(flag);
        }

        return result;

        static X509KeyStorageFlags ParseCurrentFlag(string candidate)
        {
            if (Enum.TryParse<X509KeyStorageFlags>(candidate, out var flag))
            {
                return flag;
            }
            else
            {
                throw new InvalidOperationException($"Invalid storage flag '{candidate}'");
            }
        }
    }
}