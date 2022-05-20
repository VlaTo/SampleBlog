using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SampleBlog.IdentityServer.DependencyInjection.Options;
using SampleBlog.IdentityServer.Extensions;
using SampleBlog.IdentityServer.Models;
using SampleBlog.IdentityServer.Services;
using System.Security.Cryptography;

namespace SampleBlog.IdentityServer.Core;

internal class MessageCookie<TModel>
{
    private readonly ILogger logger;
    private readonly IdentityServerOptions options;
    private readonly IHttpContextAccessor context;
    private readonly IServerUrls urls;
    private readonly IDataProtector protector;

    private string MessageType => typeof(TModel).Name;
    
    private string CookiePrefix => MessageType + ".";
    
    private string CookiePath => urls.BasePath.CleanUrlPath();
    
    private bool Secure => context.HttpContext.Request.IsHttps;

    public MessageCookie(
        ILogger<MessageCookie<TModel>> logger,
        IdentityServerOptions options,
        IHttpContextAccessor context,
        IServerUrls urls,
        IDataProtectionProvider provider)
    {
        this.logger = logger;
        this.options = options;
        this.context = context;
        this.urls = urls;
        protector = provider.CreateProtector(MessageType);
    }

    public void Write(string id, Message<TModel> message)
    {
        ClearOverflow();

        if (message == null) throw new ArgumentNullException(nameof(message));

        var name = GetCookieFullName(id);
        var data = Protect(message);

        context.HttpContext.Response.Cookies.Append(
            name,
            data,
            new CookieOptions
            {
                HttpOnly = true,
                Secure = Secure,
                Path = CookiePath,
                IsEssential = true
                // don't need to set same-site since cookie is expected to be sent
                // to only another page in this host. 
            });
    }

    public Message<TModel>? Read(string id)
    {
        if (id.IsMissing())
        {
            return null;
        }

        var name = GetCookieFullName(id);

        return ReadByCookieName(name);
    }

    protected internal void Clear(string id)
    {
        var name = GetCookieFullName(id);
        ClearByCookieName(name);
    }

    private void ClearOverflow()
    {
        var names = GetCookieNames();
        var toKeep = options.UserInteraction.CookieMessageThreshold;

        if (names.Count() >= toKeep)
        {
            var rankedCookieNames =
                from name in names
                let rank = GetCookieRank(name)
                orderby rank descending
                select name;

            var purge = rankedCookieNames.Skip(Math.Max(0, toKeep - 1));
            foreach (var name in purge)
            {
                logger.LogTrace("Purging stale cookie: {cookieName}", name);
                ClearByCookieName(name);
            }
        }
    }

    private void ClearByCookieName(string name)
    {
        context.HttpContext.Response.Cookies.Append(
            name,
            ".",
            new CookieOptions
            {
                Expires = new DateTime(2000, 1, 1),
                HttpOnly = true,
                Secure = Secure,
                Path = CookiePath,
                IsEssential = true
            });
    }

    private Message<TModel>? ReadByCookieName(string name)
    {
        var data = context.HttpContext.Request.Cookies[name];

        if (data.IsPresent())
        {
            try
            {
                return Unprotect(data);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error unprotecting message cookie");
                ClearByCookieName(name);
            }
        }

        return null;
    }


    private string Protect(Message<TModel> message)
    {
        var json = ObjectSerializer.ToString(message);
        logger.LogTrace("Protecting message: {0}", json);

        return protector.Protect(json);
    }

    private Message<TModel> Unprotect(string data)
    {
        var json = protector.Unprotect(data);
        var message = ObjectSerializer.FromString<Message<TModel>>(json);

        return message;
    }

    private string GetCookieFullName(string id) => CookiePrefix + id;

    private IEnumerable<string> GetCookieNames()
    {
        var key = CookiePrefix;

        foreach (var (name, _) in context.HttpContext.Request.Cookies)
        {
            if (name.StartsWith(key))
            {
                yield return name;
            }
        }
    }

    private long GetCookieRank(string name)
    {
        // empty and invalid cookies are considered to be the oldest:
        var rank = DateTime.MinValue.Ticks;

        try
        {
            var message = ReadByCookieName(name);
            if (message != null)
            {
                // valid cookies are ranked based on their creation time:
                rank = message.Created;
            }
        }
        catch (CryptographicException e)
        {
            // cookie was protected with a different key/algorithm
            logger.LogDebug(e, "Unable to unprotect cookie {0}", name);
        }

        return rank;
    }
}