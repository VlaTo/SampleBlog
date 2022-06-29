using Microsoft.Extensions.Logging;
using SampleBlog.IdentityServer.Core.Extensions;
using SampleBlog.IdentityServer.Services;
using SampleBlog.IdentityServer.Storage;
using SampleBlog.IdentityServer.Storage.Models;
using SampleBlog.IdentityServer.Storage.Stores;
using SampleBlog.IdentityServer.Storage.Stores.Serialization;
using System.Security.Cryptography;
using System.Text;

namespace SampleBlog.IdentityServer.Stores;

/// <summary>
/// Base class for persisting grants using the IPersistedGrantStore.
/// </summary>
/// <typeparam name="T"></typeparam>
public class DefaultGrantStore<T>
{
    private const string KeySeparator = ":";
    private const string HexEncodingFormatSuffix = "-1";

    /// <summary>
    /// The grant type being stored.
    /// </summary>
    protected string GrantType
    {
        get;
    }

    /// <summary>
    /// The PersistedGrantStore.
    /// </summary>
    protected IPersistedGrantStore Store
    {
        get;
    }

    /// <summary>
    /// The PersistentGrantSerializer;
    /// </summary>
    protected IPersistentGrantSerializer Serializer
    {
        get;
    }

    /// <summary>
    /// The HandleGenerationService.
    /// </summary>
    protected IHandleGenerationService HandleGenerationService
    {
        get;
    }

    /// <summary>
    /// The logger.
    /// </summary>
    protected ILogger Logger
    {
        get;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DefaultGrantStore{T}"/> class.
    /// </summary>
    /// <param name="grantType">Type of the grant.</param>
    /// <param name="store">The store.</param>
    /// <param name="serializer">The serializer.</param>
    /// <param name="handleGenerationService">The handle generation service.</param>
    /// <param name="logger">The logger.</param>
    /// <exception cref="System.ArgumentNullException">grantType</exception>
    protected DefaultGrantStore(
        string grantType,
        IPersistedGrantStore store,
        IPersistentGrantSerializer serializer,
        IHandleGenerationService handleGenerationService,
        ILogger logger)
    {
        GrantType = grantType;
        Store = store;
        Serializer = serializer;
        HandleGenerationService = handleGenerationService;
        Logger = logger;
    }

    /// <summary>
    /// Creates a handle.
    /// </summary>
    protected async Task<string> CreateHandleAsync()
    {
        var handle = await HandleGenerationService.GenerateAsync();
        return handle + HexEncodingFormatSuffix;
    }

    /// <summary>
    /// Gets the hashed key.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns></returns>
    protected virtual string GetHashedKey(string value)
    {
        var key = (value + KeySeparator + GrantType);

        if (value.EndsWith(HexEncodingFormatSuffix))
        {
            // newer format >= v6; uses hex encoding to avoid collation issues
            using (var sha = SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(key);
                var hash = sha.ComputeHash(bytes);
                return BitConverter.ToString(hash).Replace("-", "");
            }
        }

        // old format <= v5
        return key.Sha256();
    }

    /// <summary>
    /// Gets the item.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <returns></returns>
    protected virtual async Task<T?> GetItemAsync(string key)
    {
        var hashedKey = GetHashedKey(key);
        var item = await GetItemByHashedKeyAsync(hashedKey);

        if (null == item)
        {
            Logger.LogDebug("{grantType} grant with value: {key} not found in store.", GrantType, key);
        }

        return item;
    }

    /// <summary>
    /// Gets the item by the hashed key.
    /// </summary>
    /// <param name="hashedKey"></param>
    /// <returns></returns>
    protected virtual async Task<T?> GetItemByHashedKeyAsync(string hashedKey)
    {
        var grant = await Store.GetAsync(hashedKey);

        if (grant != null && grant.Type == GrantType)
        {
            try
            {
                return Serializer.Deserialize<T>(grant.Data);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Failed to deserialize JSON from grant store.");
            }
        }

        return default;
    }

    /// <summary>
    /// Gets the items.
    /// </summary>
    protected virtual async Task<IEnumerable<T>?> GetAllAsync(PersistedGrantFilter filter)
    {
        //filter.Type = GrantType;

        var items = await Store.GetAllAsync(filter);
        var result = items.Select(x => Serializer.Deserialize<T>(x.Data)).ToArray();
        
        return result;
    }

    /// <summary>
    /// Creates the item.
    /// </summary>
    /// <param name="item">The item.</param>
    /// <param name="clientId">The client identifier.</param>
    /// <param name="subjectId">The subject identifier.</param>
    /// <param name="sessionId">The session identifier.</param>
    /// <param name="description">The description.</param>
    /// <param name="created">The created.</param>
    /// <param name="lifetime">The lifetime.</param>
    /// <returns></returns>
    protected virtual async Task<string> CreateItemAsync(
        T item,
        string clientId,
        string subjectId,
        string sessionId,
        string description,
        DateTime created,
        TimeSpan lifetime)
    {
        var handle = await CreateHandleAsync();
        var expiration = created.Add(lifetime);

        await StoreItemAsync(handle, item, clientId, subjectId, sessionId, description, created, expiration);

        return handle;
    }

    /// <summary>
    /// Stores the item.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <param name="item">The item.</param>
    /// <param name="clientId">The client identifier.</param>
    /// <param name="subjectId">The subject identifier.</param>
    /// <param name="sessionId">The session identifier.</param>
    /// <param name="description">The description.</param>
    /// <param name="created">The created time.</param>
    /// <param name="expiration">The expiration.</param>
    /// <param name="consumedTime">The consumed time.</param>
    /// <returns></returns>
    protected virtual Task StoreItemAsync(
        string key,
        T item,
        string clientId,
        string subjectId,
        string sessionId,
        string description,
        DateTime created,
        DateTime? expiration,
        DateTime? consumedTime = null)
    {
        key = GetHashedKey(key);
        return StoreItemByHashedKeyAsync(key, item, clientId, subjectId, sessionId, description, created, expiration, consumedTime);
    }

    /// <summary>
    /// Stores the item.
    /// </summary>
    /// <param name="hashedKey">The key.</param>
    /// <param name="item">The item.</param>
    /// <param name="clientId">The client identifier.</param>
    /// <param name="subjectId">The subject identifier.</param>
    /// <param name="sessionId">The session identifier.</param>
    /// <param name="description">The description.</param>
    /// <param name="created">The created time.</param>
    /// <param name="expiration">The expiration.</param>
    /// <param name="consumedTime">The consumed time.</param>
    /// <returns></returns>
    protected virtual async Task StoreItemByHashedKeyAsync(
        string hashedKey,
        T item,
        string clientId,
        string subjectId,
        string? sessionId,
        string? description,
        DateTime created,
        DateTime? expiration,
        DateTime? consumedTime = null)
    {
        var json = Serializer.Serialize(item);

        var grant = new PersistedGrant
        {
            Key = hashedKey,
            Type = GrantType,
            ClientId = clientId,
            SubjectId = subjectId,
            SessionId = sessionId,
            Description = description,
            CreationTime = created,
            Expiration = expiration,
            ConsumedTime = consumedTime,
            Data = json
        };

        await Store.StoreAsync(grant);
    }

    /// <summary>
    /// Removes the item.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <returns></returns>
    protected virtual Task RemoveItemAsync(string key)
    {
        key = GetHashedKey(key);
        return RemoveItemByHashedKeyAsync(key);
    }

    /// <summary>
    /// Removes the item.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <returns></returns>
    protected virtual async Task RemoveItemByHashedKeyAsync(string key)
    {
        await Store.RemoveAsync(key);
    }

    /// <summary>
    /// Removes all items for a subject id / client id combination.
    /// </summary>
    /// <param name="subjectId">The subject identifier.</param>
    /// <param name="clientId">The client identifier.</param>
    /// <returns></returns>
    protected virtual async Task RemoveAllAsync(string subjectId, string clientId)
    {
        await Store.RemoveAllAsync(new PersistedGrantFilter(subjectId: subjectId, clientId: clientId, type: GrantType));
    }
}