using IdentityModel;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SampleBlog.IdentityServer.Contexts;
using SampleBlog.IdentityServer.Core;
using SampleBlog.IdentityServer.DependencyInjection.Options;
using SampleBlog.IdentityServer.Endpoints;
using SampleBlog.IdentityServer.Extensions;
using SampleBlog.IdentityServer.Hosting;
using SampleBlog.IdentityServer.ResponseHandling;
using SampleBlog.IdentityServer.ResponseHandling.Defaults;
using SampleBlog.IdentityServer.Services;
using SampleBlog.IdentityServer.Storage.Services;
using SampleBlog.IdentityServer.Storage.Stores;
using SampleBlog.IdentityServer.Storage.Stores.Serialization;
using SampleBlog.IdentityServer.Stores;
using SampleBlog.IdentityServer.Validation;

namespace SampleBlog.IdentityServer.DependencyInjection.Extensions;

public static class IdentityServerBuilderCoreExtensions
{
    /// <summary>
    /// Adds the required platform services.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <returns></returns>
    public static IIdentityServerBuilder AddRequiredPlatformServices(this IIdentityServerBuilder builder)
    {
        builder.Services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        builder.Services.TryAddTransient<IScopeParser, DefaultScopeParser>();
        builder.Services.TryAddTransient<IAuthorizeRequestValidator, AuthorizeRequestValidator>();
        builder.Services.TryAddTransient<IClientConfigurationValidator, DefaultClientConfigurationValidator>();
        builder.Services.TryAddTransient<IEventService, DefaultEventService>();
        builder.Services.TryAddTransient<IEventSink, DefaultEventSink>();
        builder.Services.TryAddTransient<ICustomAuthorizeRequestValidator, DefaultCustomAuthorizeRequestValidator>();
        builder.Services.TryAddTransient<IResourceValidator, DefaultResourceValidator>();
        builder.Services.AddOptions();
        builder.Services.AddSingleton(
            resolver => resolver.GetRequiredService<IOptions<IdentityServerOptions>>().Value
        );
        builder.Services.AddSingleton(
            resolver => resolver.GetRequiredService<IOptions<IdentityServerOptions>>().Value.DynamicProviders
        );
        //builder.Services.AddTransient(
        //    resolver => resolver.GetRequiredService<IOptions<IdentityServerOptions>>().Value.PersistentGrants
        //);

        builder.Services.AddHttpClient();

        return builder;
    }

    public static IIdentityServerBuilder AddCookieAuthentication(this IIdentityServerBuilder builder)
    {
        return builder
            .AddDefaultCookieHandlers()
            .AddCookieAuthenticationExtensions();
    }

    /// <summary>
    /// Adds a persisted grant store.
    /// </summary>
    /// <typeparam name="T">The type of the concrete grant store that is registered in DI.</typeparam>
    /// <param name="builder">The builder.</param>
    /// <returns>The builder.</returns>
    public static IIdentityServerBuilder AddPersistedGrantStore<T>(this IIdentityServerBuilder builder)
        where T : class, IPersistedGrantStore
    {
        builder.Services.AddTransient<IPersistedGrantStore, T>();

        return builder;
    }

    /// <summary>
    /// Adds the extension grant validator.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="builder">The builder.</param>
    /// <returns></returns>
    public static IIdentityServerBuilder AddExtensionGrantValidator<T>(this IIdentityServerBuilder builder)
        where T : class, IExtensionGrantValidator
    {
        builder.Services.AddTransient<IExtensionGrantValidator, T>();

        return builder;
    }

    /// <summary>
    /// Adds the default cookie handlers and corresponding configuration
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <returns></returns>
    public static IIdentityServerBuilder AddDefaultCookieHandlers(this IIdentityServerBuilder builder)
    {
        builder.Services
            .AddAuthentication(IdentityServerConstants.DefaultCookieAuthenticationScheme)
            .AddCookie(IdentityServerConstants.DefaultCookieAuthenticationScheme)
            .AddCookie(IdentityServerConstants.ExternalCookieAuthenticationScheme);

        builder.Services
            .AddSingleton<IConfigureOptions<CookieAuthenticationOptions>, ConfigureInternalCookieOptions>();

        return builder;
    }

    /// <summary>
    /// Adds the necessary decorators for cookie authentication required by IdentityServer
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <returns></returns>
    public static IIdentityServerBuilder AddCookieAuthenticationExtensions(this IIdentityServerBuilder builder)
    {
        builder.Services.AddSingleton<IPostConfigureOptions<CookieAuthenticationOptions>, PostConfigureInternalCookieOptions>();
        builder.Services.AddTransientDecorator<IAuthenticationService, IdentityServerAuthenticationService>();
        builder.Services.AddTransientDecorator<IAuthenticationHandlerProvider, FederatedSignoutAuthenticationHandlerProvider>();

        return builder;
    }

    public static IIdentityServerBuilder AddCoreServices(this IIdentityServerBuilder builder)
    {
        builder.Services.AddTransient<IServerUrls, DefaultServerUrls>();
        builder.Services.AddTransient<IIssuerNameService, DefaultIssuerNameService>();
        builder.Services.AddTransient<ISecretsListParser, SecretParser>();
        //builder.Services.AddTransient<ISecretsListValidator, SecretValidator>();
        builder.Services.AddTransient<ExtensionGrantValidator>();
        //builder.Services.AddTransient<BearerTokenUsageValidator>();
        builder.Services.AddTransient<IJwtRequestValidator, JwtRequestValidator>();

        builder.Services.AddTransient<ReturnUrlParser>();
        //builder.Services.AddTransient<IdentityServerTools>();

        builder.Services.AddTransient<IReturnUrlParser, OidcReturnUrlParser>();
        builder.Services.AddScoped<IUserSession, DefaultUserSession>();
        //builder.Services.AddTransient(typeof(MessageCookie<>));

        builder.Services.AddCors();
        builder.Services.AddTransientDecorator<ICorsPolicyProvider, CorsPolicyProvider>();

        builder.Services.TryAddTransient<ICorsPolicyService, DefaultCorsPolicyService>();
        builder.Services.TryAddTransient<ICancellationTokenProvider, DefaultHttpContextCancellationTokenProvider>();
        builder.Services.TryAddTransient<IMessageStore<LogoutNotificationContext>, ProtectedDataMessageStore<LogoutNotificationContext>>();
        builder.Services.TryAddTransient<IAuthorizationCodeStore, DefaultAuthorizationCodeStore>();
        builder.Services.TryAddTransient<IReferenceTokenStore, DefaultReferenceTokenStore>();
        builder.Services.TryAddTransient<IKeyMaterialService, DefaultKeyMaterialService>();
        builder.Services.TryAddTransient<ILogoutNotificationService, LogoutNotificationService>();
        builder.Services.TryAddTransient<ICustomTokenValidator, DefaultCustomTokenValidator>();
        builder.Services.TryAddTransient<ITokenValidator, TokenValidator>();
        builder.Services.TryAddTransient<IEndSessionRequestValidator, EndSessionRequestValidator>();
        builder.Services.TryAddTransient<IPersistentGrantSerializer, PersistentGrantSerializer>();
        builder.Services.TryAddTransient<IHandleGenerationService, DefaultHandleGenerationService>();
        builder.Services.TryAddTransient<ISessionCoordinationService, DefaultSessionCoordinationService>();

        builder.AddJwtRequestUriHttpClient();

        //builder.Services.TryAddTransient<IBackchannelAuthenticationUserValidator, NopBackchannelAuthenticationUserValidator>();

        //builder.Services.TryAddTransient(typeof(IConcurrencyLock<>), typeof(DefaultConcurrencyLock<>));

        return builder;
    }

    /// <summary>
    /// Adds the default endpoints.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <returns></returns>
    public static IIdentityServerBuilder AddDefaultEndpoints(this IIdentityServerBuilder builder)
    {
        builder.Services.AddTransient<IEndpointRouter, EndpointRouter>();

        //builder.AddEndpoint<AuthorizeCallbackEndpoint>(EndpointNames.Authorize, ProtocolRoutePaths.AuthorizeCallback.EnsureLeadingSlash());
        //builder.AddEndpoint<AuthorizeEndpoint>(EndpointNames.Authorize, ProtocolRoutePaths.Authorize.EnsureLeadingSlash());
        //builder.AddEndpoint<BackchannelAuthenticationEndpoint>(EndpointNames.BackchannelAuthentication, ProtocolRoutePaths.BackchannelAuthentication.EnsureLeadingSlash());
        //builder.AddEndpoint<CheckSessionEndpoint>(EndpointNames.CheckSession, ProtocolRoutePaths.CheckSession.EnsureLeadingSlash());
        //builder.AddEndpoint<DeviceAuthorizationEndpoint>(EndpointNames.DeviceAuthorization, ProtocolRoutePaths.DeviceAuthorization.EnsureLeadingSlash());
        //builder.AddEndpoint<DiscoveryKeyEndpoint>(EndpointNames.Discovery, ProtocolRoutePaths.DiscoveryWebKeys.EnsureLeadingSlash());
        builder.AddEndpoint<DiscoveryEndpoint>(Constants.EndpointNames.Discovery, Constants.ProtocolRoutePaths.DiscoveryConfiguration.EnsureLeadingSlash());
        //builder.AddEndpoint<EndSessionCallbackEndpoint>(EndpointNames.EndSession, ProtocolRoutePaths.EndSessionCallback.EnsureLeadingSlash());
        //builder.AddEndpoint<EndSessionEndpoint>(EndpointNames.EndSession, ProtocolRoutePaths.EndSession.EnsureLeadingSlash());
        //builder.AddEndpoint<IntrospectionEndpoint>(EndpointNames.Introspection, ProtocolRoutePaths.Introspection.EnsureLeadingSlash());
        //builder.AddEndpoint<TokenRevocationEndpoint>(EndpointNames.Revocation, ProtocolRoutePaths.Revocation.EnsureLeadingSlash());
        //builder.AddEndpoint<TokenEndpoint>(EndpointNames.Token, ProtocolRoutePaths.Token.EnsureLeadingSlash());
        //builder.AddEndpoint<UserInfoEndpoint>(EndpointNames.UserInfo, ProtocolRoutePaths.UserInfo.EnsureLeadingSlash());

        return builder;
    }

    /// <summary>
    /// Adds the secret parser.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="builder">The builder.</param>
    /// <returns></returns>
    public static IIdentityServerBuilder AddSecretParser<T>(this IIdentityServerBuilder builder)
        where T : class, ISecretParser
    {
        builder.Services.AddTransient<ISecretParser, T>();

        return builder;
    }

    /// <summary>
    /// Adds a CORS policy service.
    /// </summary>
    /// <typeparam name="T">The type of the concrete scope store class that is registered in DI.</typeparam>
    /// <param name="builder">The builder.</param>
    /// <returns></returns>
    public static IIdentityServerBuilder AddCorsPolicyService<T>(this IIdentityServerBuilder builder)
        where T : class, ICorsPolicyService
    {
        builder.Services.AddTransient<ICorsPolicyService, T>();
        return builder;
    }

    /// <summary>
    /// Configures IdentityServer to use the ASP.NET Identity implementations 
    /// of IUserClaimsPrincipalFactory, IResourceOwnerPasswordValidator, and IProfileService.
    /// Also configures some of ASP.NET Identity's options for use with IdentityServer (such as claim types to use
    /// and authentication cookie settings).
    /// </summary>
    /// <typeparam name="TUser">The type of the user.</typeparam>
    /// <param name="builder">The builder.</param>
    /// <returns></returns>
    public static IIdentityServerBuilder AddAspNetIdentity<TUser>(this IIdentityServerBuilder builder)
        where TUser : class
    {
        builder.Services.AddTransientDecorator<IUserClaimsPrincipalFactory<TUser>, UserClaimsFactory<TUser>>();

        builder.Services.Configure<IdentityOptions>(options =>
        {
            options.ClaimsIdentity.UserIdClaimType = JwtClaimTypes.Subject;
            options.ClaimsIdentity.UserNameClaimType = JwtClaimTypes.Name;
            options.ClaimsIdentity.RoleClaimType = JwtClaimTypes.Role;
            options.ClaimsIdentity.EmailClaimType = JwtClaimTypes.Email;
        });

        builder.Services.Configure<SecurityStampValidatorOptions>(opts =>
        {
            opts.OnRefreshingPrincipal = SecurityStampValidatorCallback.UpdatePrincipal;
        });

        builder.Services.ConfigureApplicationCookie(options =>
        {
            options.Cookie.IsEssential = true;
            // we need to disable to allow iframe for authorize requests
            options.Cookie.SameSite = SameSiteMode.None;
        });

        builder.Services.ConfigureExternalCookie(options =>
        {
            options.Cookie.IsEssential = true;
            // https://github.com/IdentityServer/IdentityServer4/issues/2595
            options.Cookie.SameSite = SameSiteMode.None;
        });

        builder.Services.Configure<CookieAuthenticationOptions>(IdentityConstants.TwoFactorRememberMeScheme, options =>
        {
            options.Cookie.IsEssential = true;
        });

        builder.Services.Configure<CookieAuthenticationOptions>(IdentityConstants.TwoFactorUserIdScheme, options =>
        {
            options.Cookie.IsEssential = true;
        });

        builder.Services.AddAuthentication(options =>
        {
            if (options.DefaultAuthenticateScheme == null &&
                options.DefaultScheme == IdentityServerConstants.DefaultCookieAuthenticationScheme)
            {
                options.DefaultScheme = IdentityConstants.ApplicationScheme;
            }
        });

        builder.AddResourceOwnerValidator<ResourceOwnerPasswordValidator<TUser>>();
        builder.AddProfileService<ProfileService<TUser>>();

        return builder;
    }

    /// <summary>
    /// Adds a client store.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="builder">The builder.</param>
    /// <returns></returns>
    public static IIdentityServerBuilder AddClientStore<T>(this IIdentityServerBuilder builder)
        where T : class, IClientStore
    {
        builder.Services.TryAddTransient(typeof(T));
        builder.Services.AddTransient<IClientStore, ValidatingClientStore<T>>();

        return builder;
    }

    /// <summary>
    /// Adds a resource store.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="builder">The builder.</param>
    /// <returns></returns>
    public static IIdentityServerBuilder AddResourceStore<T>(this IIdentityServerBuilder builder)
        where T : class, IResourceStore
    {
        builder.Services.AddTransient<IResourceStore, T>();

        return builder;
    }

    /// <summary>
    /// Adds the resource owner validator.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="builder">The builder.</param>
    /// <returns></returns>
    public static IIdentityServerBuilder AddResourceOwnerValidator<T>(this IIdentityServerBuilder builder)
        where T : class, IResourceOwnerPasswordValidator
    {
        builder.Services.AddTransient<IResourceOwnerPasswordValidator, T>();

        return builder;
    }

    /// <summary>
    /// Adds the profile service.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="builder">The builder.</param>
    /// <returns></returns>
    public static IIdentityServerBuilder AddProfileService<T>(this IIdentityServerBuilder builder)
        where T : class, IProfileService
    {
        builder.Services.AddTransient<IProfileService, T>();

        return builder;
    }

    /// <summary>
    /// Adds the endpoint.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="builder">The builder.</param>
    /// <param name="name">The name.</param>
    /// <param name="path">The path.</param>
    /// <returns></returns>
    public static IIdentityServerBuilder AddEndpoint<T>(this IIdentityServerBuilder builder, string name, PathString path)
        where T : class, IEndpointHandler
    {
        builder.Services.AddTransient<T>();
        builder.Services.AddSingleton(new Hosting.Endpoint(name, path, typeof(T)));

        return builder;
    }

    // todo: check with later previews of ASP.NET Core if this is still required
    /// <summary>
    /// Adds configuration for the HttpClient used for JWT request_uri requests.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <param name="configureClient">The configuration callback.</param>
    /// <returns></returns>
    public static IHttpClientBuilder AddJwtRequestUriHttpClient(this IIdentityServerBuilder builder, Action<HttpClient>? configureClient = null)
    {
        const string name = IdentityServerConstants.HttpClients.JwtRequestUriHttpClient;

        IHttpClientBuilder httpBuilder;

        if (null != configureClient)
        {
            httpBuilder = builder.Services.AddHttpClient(name, configureClient);
        }
        else
        {
            httpBuilder = builder.Services.AddHttpClient(name).ConfigureHttpClient(client =>
                client.Timeout = TimeSpan.FromSeconds(IdentityServerConstants.HttpClients.DefaultTimeoutSeconds)
            );
        }

        builder.Services.AddTransient<IJwtRequestUriHttpClient, DefaultJwtRequestUriHttpClient>(s =>
        {
            var httpClientFactory = s.GetRequiredService<IHttpClientFactory>();
            var httpClient = httpClientFactory.CreateClient(name);
            var loggerFactory = s.GetRequiredService<ILoggerFactory>();
            var options = s.GetRequiredService<IdentityServerOptions>();

            return new DefaultJwtRequestUriHttpClient(httpClient, options, new NoneCancellationTokenProvider(), loggerFactory);
        });

        return httpBuilder;
    }

    /// <summary>
    /// Adds the response generators.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <returns></returns>
    public static IIdentityServerBuilder AddResponseGenerators(this IIdentityServerBuilder builder)
    {
        //builder.Services.TryAddTransient<ITokenResponseGenerator, TokenResponseGenerator>();
        //builder.Services.TryAddTransient<IUserInfoResponseGenerator, UserInfoResponseGenerator>();
        //builder.Services.TryAddTransient<IIntrospectionResponseGenerator, IntrospectionResponseGenerator>();
        //builder.Services.TryAddTransient<IAuthorizeInteractionResponseGenerator, AuthorizeInteractionResponseGenerator>();
        builder.Services.TryAddTransient<IAuthorizeResponseGenerator, AuthorizeResponseGenerator>();
        builder.Services.TryAddTransient<IDiscoveryResponseGenerator, DiscoveryResponseGenerator>();
        //builder.Services.TryAddTransient<ITokenRevocationResponseGenerator, TokenRevocationResponseGenerator>();
        //builder.Services.TryAddTransient<IDeviceAuthorizationResponseGenerator, DeviceAuthorizationResponseGenerator>();
        //builder.Services.TryAddTransient<IBackchannelAuthenticationResponseGenerator, BackchannelAuthenticationResponseGenerator>();

        return builder;
    }

    /// <summary>
    /// Adds the default secret parsers.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <returns></returns>
    public static IIdentityServerBuilder AddDefaultSecretParsers(this IIdentityServerBuilder builder)
    {
        builder.Services.AddTransient<ISecretParser, BasicAuthenticationSecretParser>();
        builder.Services.AddTransient<ISecretParser, PostBodySecretParser>();

        return builder;
    }

    internal static void AddTransientDecorator<TService, TImplementation>(this IServiceCollection services)
        where TService : class
        where TImplementation : class, TService
    {
        services.AddDecorator<TService>();
        services.AddTransient<TService, TImplementation>();
    }

    internal static void AddDecorator<TService>(this IServiceCollection services)
    {
        var registration = services.LastOrDefault(x => x.ServiceType == typeof(TService));
        
        if (null == registration)
        {
            throw new InvalidOperationException("Service type: " + typeof(TService).Name + " not registered.");
        }

        if (services.Any(x => x.ServiceType == typeof(Decorator<TService>)))
        {
            throw new InvalidOperationException("Decorator already registered for type: " + typeof(TService).Name + ".");
        }

        services.Remove(registration);

        if (null != registration.ImplementationInstance)
        {
            var type = registration.ImplementationInstance.GetType();
            var innerType = typeof(Decorator<,>).MakeGenericType(typeof(TService), type);

            services.Add(new ServiceDescriptor(typeof(Decorator<TService>), innerType, ServiceLifetime.Transient));
            services.Add(new ServiceDescriptor(type, registration.ImplementationInstance));
        }
        else if (null != registration.ImplementationFactory)
        {
            services.Add(new ServiceDescriptor(
                typeof(Decorator<TService>),
                provider => Decorator<TService>.CreateDisposable((TService)registration.ImplementationFactory(provider)),
                registration.Lifetime)
            );
        }
        else
        {
            var type = registration.ImplementationType;
            var innerType = typeof(Decorator<,>).MakeGenericType(typeof(TService), registration.ImplementationType!);
            services.Add(new ServiceDescriptor(typeof(Decorator<TService>), innerType, ServiceLifetime.Transient));
            services.Add(new ServiceDescriptor(type!, type!, registration.Lifetime));
        }
    }
}