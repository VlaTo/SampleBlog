using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using SampleBlog.IdentityServer.Storage.Models;

namespace SampleBlog.IdentityServer.DependencyInjection.Options;

/// <summary>
/// Configures the dynamic external provider feature.
/// </summary>
public class DynamicProviderOptions
{
    private readonly Dictionary<string, DynamicProviderType> providers;

    /// <summary>
    /// Prefix in the pipeline for callbacks from external providers. Defaults to "/federation".
    /// </summary>
    public PathString PathPrefix
    {
        get;
        set;
    }

    /// <summary>
    /// Scheme used for signin. Defaults to the constant IdentityServerConstants.ExternalCookieAuthenticationScheme.
    /// </summary>
    public string SignInScheme
    {
        get;
        set;
    }

    /// <summary>
    /// Scheme for signout. Defaults to the constant IdentityServerConstants.DefaultCookieAuthenticationScheme.
    /// </summary>
    public string SignOutScheme
    {
        get;
        set;
    }

    public DynamicProviderOptions()
    {
        providers = new Dictionary<string, DynamicProviderType>();
        PathPrefix = "/federation";
        SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;
        SignOutScheme = IdentityServerConstants.DefaultCookieAuthenticationScheme;
    }

    /// <summary>
    /// Registers a provider configuration model and authentication handler for the protocol type being used.
    /// </summary>
    public void AddProviderType<THandler, TOptions, TIdentityProvider>(string type)
        where THandler : IAuthenticationRequestHandler
        where TOptions : AuthenticationSchemeOptions, new()
        where TIdentityProvider : IdentityProvider
    {
        if (providers.ContainsKey(type))
        {
            throw new Exception($"Type '{type}' already configured.");
        }

        providers.Add(type, new DynamicProviderType
        {
            HandlerType = typeof(THandler),
            OptionsType = typeof(TOptions),
            IdentityProviderType = typeof(TIdentityProvider),
        });
    }

    /// <summary>
    /// Finds the DynamicProviderType registration by protocol type.
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public DynamicProviderType? FindProviderType(string type)
    {
        return providers.ContainsKey(type) ? providers[type] : null;
    }

    #region DynamicProviderType

    /// <summary>
    /// Models a provider type registered with the dynamic providers feature.
    /// </summary>
    public class DynamicProviderType
    {
        /// <summary>
        /// The type of the handler.
        /// </summary>
        public Type HandlerType
        {
            get;
            set;
        }

        /// <summary>
        /// The type of the options.
        /// </summary>
        public Type OptionsType
        {
            get;
            set;
        }

        /// <summary>
        /// The identity provider protocol type.
        /// </summary>
        public Type IdentityProviderType
        {
            get;
            set;
        }
    }

    #endregion
}