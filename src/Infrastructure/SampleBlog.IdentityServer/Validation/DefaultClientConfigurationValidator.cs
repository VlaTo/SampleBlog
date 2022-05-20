using SampleBlog.IdentityServer.Core;
using SampleBlog.IdentityServer.DependencyInjection.Options;
using SampleBlog.IdentityServer.Validation.Contexts;

namespace SampleBlog.IdentityServer.Validation;

/// <summary>
/// Default client configuration validator
/// </summary>
/// <seealso cref="IClientConfigurationValidator" />
public class DefaultClientConfigurationValidator : IClientConfigurationValidator
{
    private readonly IdentityServerOptions options;

    /// <summary>
    /// Constructor for DefaultClientConfigurationValidator
    /// </summary>
    public DefaultClientConfigurationValidator(IdentityServerOptions options)
    {
        this.options = options;
    }

    /// <summary>
    /// Determines whether the configuration of a client is valid.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <returns></returns>
    public async Task ValidateAsync(ClientConfigurationValidationContext context)
    {
        using var activity = Tracing.ValidationActivitySource.StartActivity("DefaultClientConfigurationValidator.Validate");

        if (IdentityServerConstants.ProtocolTypes.OpenIdConnect == context.Client.ProtocolType)
        {
            await ValidateGrantTypesAsync(context);

            if (false == context.IsValid)
            {
                return;
            }

            await ValidateLifetimesAsync(context);

            if (false == context.IsValid)
            {
                return;
            }

            await ValidateRedirectUriAsync(context);

            if (false == context.IsValid)
            {
                return;
            }

            await ValidateAllowedCorsOriginsAsync(context);

            if (false == context.IsValid)
            {
                return;
            }

            await ValidateUriSchemesAsync(context);

            if (false == context.IsValid)
            {
                return;
            }

            await ValidateSecretsAsync(context);

            if (false == context.IsValid)
            {
                return;
            }

            await ValidatePropertiesAsync(context);

            if (false == context.IsValid)
            {
                return;
            }
        }
    }

    /// <summary>
    /// Validates grant type related configuration settings.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <returns></returns>
    protected virtual Task ValidateGrantTypesAsync(ClientConfigurationValidationContext context)
    {
        if (false == context.Client.AllowedGrantTypes.Any())
        {
            context.SetError("no allowed grant type specified");
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Validates lifetime related configuration settings.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <returns></returns>
    protected virtual Task ValidateLifetimesAsync(ClientConfigurationValidationContext context)
    {
        var client = context.Client;

        if (TimeSpan.Zero >= client.AccessTokenLifetime)
        {
            context.SetError("access token lifetime is 0 or negative");
            return Task.CompletedTask;
        }

        if (TimeSpan.Zero >= client.IdentityTokenLifetime)
        {
            context.SetError("identity token lifetime is 0 or negative");
            return Task.CompletedTask;
        }

        if (client.AllowedGrantTypes.Contains(GrantType.DeviceFlow) && TimeSpan.Zero >= client.DeviceCodeLifetime)
        {
            context.SetError("device code lifetime is 0 or negative");
        }

        // 0 means unlimited lifetime
        if (TimeSpan.Zero > client.AbsoluteRefreshTokenLifetime)
        {
            context.SetError("absolute refresh token lifetime is negative");
            return Task.CompletedTask;
        }

        // 0 might mean that sliding is disabled
        if (TimeSpan.Zero > client.SlidingRefreshTokenLifetime)
        {
            context.SetError("sliding refresh token lifetime is negative");
            return Task.CompletedTask;
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Validates redirect URI related configuration.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <returns></returns>
    protected virtual Task ValidateRedirectUriAsync(ClientConfigurationValidationContext context)
    {
        var grantTypes = context.Client.AllowedGrantTypes;

        if (grantTypes.Any())
        {
            if (grantTypes.Contains(GrantType.AuthorizationCode) || grantTypes.Contains(GrantType.Hybrid) || grantTypes.Contains(GrantType.Implicit))
            {
                if (false == context.Client.RedirectUris.Any())
                {
                    context.SetError("No redirect URI configured.");
                }
            }
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Validates allowed CORS origins for valid format.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <returns></returns>
    protected virtual Task ValidateAllowedCorsOriginsAsync(ClientConfigurationValidationContext context)
    {
        if (context.Client.AllowedCorsOrigins.Any())
        {
            foreach (var origin in context.Client.AllowedCorsOrigins)
            {
                var fail = true;

                if (false == String.IsNullOrWhiteSpace(origin) && Uri.IsWellFormedUriString(origin, UriKind.Absolute))
                {
                    var uri = new Uri(origin);

                    if (uri.AbsolutePath == "/" && !origin.EndsWith("/"))
                    {
                        fail = false;
                    }
                }

                if (fail)
                {
                    context.SetError(String.IsNullOrWhiteSpace(origin)
                        ? "AllowedCorsOrigins contains invalid origin. There is an empty value."
                        : $"AllowedCorsOrigins contains invalid origin: {origin}"
                    );

                    return Task.CompletedTask;
                }
            }
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Validates that URI schemes is not in the list of invalid URI scheme prefixes, as controlled by the ValidationOptions.
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    protected virtual Task ValidateUriSchemesAsync(ClientConfigurationValidationContext context)
    {
        if (context.Client.RedirectUris.Any())
        {
            foreach (var uri in context.Client.RedirectUris)
            {
                var exists = options.Validation.InvalidRedirectUriPrefixes.Any(
                    scheme => uri.StartsWith(scheme, StringComparison.OrdinalIgnoreCase)
                );

                if (exists)
                {
                    context.SetError($"RedirectUri '{uri}' uses invalid scheme. If this scheme should be allowed, then configure it via ValidationOptions.");
                }
            }
        }

        if (context.Client.PostLogoutRedirectUris.Any())
        {
            foreach (var uri in context.Client.PostLogoutRedirectUris)
            {
                var exists = options.Validation.InvalidRedirectUriPrefixes.Any(
                    scheme => uri.StartsWith(scheme, StringComparison.OrdinalIgnoreCase)
                );

                if (exists)
                {
                    context.SetError($"PostLogoutRedirectUri '{uri}' uses invalid scheme. If this scheme should be allowed, then configure it via ValidationOptions.");
                }
            }
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Validates secret related configuration.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <returns></returns>
    protected virtual Task ValidateSecretsAsync(ClientConfigurationValidationContext context)
    {
        if (context.Client.AllowedGrantTypes.Any())
        {
            foreach (var grantType in context.Client.AllowedGrantTypes)
            {
                if (String.Equals(grantType, GrantType.Implicit))
                {
                    continue;
                }

                if (context.Client.RequireClientSecret && 0 == context.Client.ClientSecrets.Count)
                {
                    context.SetError($"Client secret is required for {grantType}, but no client secret is configured.");
                    return Task.CompletedTask;
                }
            }
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Validates properties related configuration settings.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <returns></returns>
    protected virtual Task ValidatePropertiesAsync(ClientConfigurationValidationContext context)
    {
        return Task.CompletedTask;
    }
}