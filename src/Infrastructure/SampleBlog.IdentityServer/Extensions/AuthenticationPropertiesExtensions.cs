using IdentityModel;
using Microsoft.AspNetCore.Authentication;
using SampleBlog.IdentityServer.Core;
using System.Text;

namespace SampleBlog.IdentityServer.Extensions;

public static class AuthenticationPropertiesExtensions
{
    internal const string SessionIdKey = "session_id";
    internal const string ClientListKey = "client_list";

    /// <summary>
    /// Adds a client to the list of clients the user has signed into during their session.
    /// </summary>
    /// <param name="properties"></param>
    /// <param name="clientId"></param>
    public static void AddClientId(this AuthenticationProperties properties, string clientId)
    {
        if (null == clientId)
        {
            throw new ArgumentNullException(nameof(clientId));
        }

        var clients = properties.GetClientList();

        if (false == clients.Contains(clientId))
        {
            var update = clients.ToList();
            
            update.Add(clientId);

            properties.SetClientList(update);
        }
    }

    /// <summary>
    /// Gets the user's session identifier.
    /// </summary>
    /// <param name="properties"></param>
    /// <returns></returns>
    public static string? GetSessionId(this AuthenticationProperties properties)
    {
        return properties.Items.TryGetValue(SessionIdKey, out var value) ? value : null;
    }

    /// <summary>
    /// Sets the user's session identifier.
    /// </summary>
    /// <param name="properties"></param>
    /// <param name="sid">The session id</param>
    /// <returns></returns>
    public static void SetSessionId(this AuthenticationProperties properties, string sid)
    {
        properties.Items[SessionIdKey] = sid;
    }

    /// <summary>
    /// Gets the list of client ids the user has signed into during their session.
    /// </summary>
    /// <param name="properties"></param>
    /// <returns></returns>
    public static string[] GetClientList(this AuthenticationProperties properties)
    {
        if (properties.Items.TryGetValue(ClientListKey, out var value))
        {
            return DecodeList(value);
        }

        return Array.Empty<string>();
    }

    /// <summary>
    /// Sets the list of client ids.
    /// </summary>
    /// <param name="properties"></param>
    /// <param name="clientIds"></param>
    public static void SetClientList(this AuthenticationProperties properties, IEnumerable<string> clientIds)
    {
        var value = EncodeList(clientIds);
        if (value == null)
        {
            properties.Items.Remove(ClientListKey);
        }
        else
        {
            properties.Items[ClientListKey] = value;
        }
    }

    private static string? EncodeList(IEnumerable<string>? list)
    {
        if (null != list && list.Any())
        {
            var value = ObjectSerializer.ToString(list);
            var bytes = Encoding.UTF8.GetBytes(value);
            value = Base64Url.Encode(bytes);
            return value;
        }

        return null;
    }

    private static string[] DecodeList(string? value)
    {
        if (value.IsPresent())
        {
            var bytes = Base64Url.Decode(value);
            
            value = Encoding.UTF8.GetString(bytes);

            var strings = ObjectSerializer.FromString<string[]>(value);

            if (null != strings)
            {
                return strings;
            }
        }

        return Array.Empty<string>();
    }
}