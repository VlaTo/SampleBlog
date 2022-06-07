namespace SampleBlog.IdentityServer.DependencyInjection.Options;

public class IdentityServerOptions
{
    /// <summary>
    /// Gets or sets the unique name of this server instance, e.g. https://myissuer.com.
    /// If not set, the issuer name is inferred from the request
    /// </summary>
    /// <value>
    /// Unique name of this server instance, e.g. https://myissuer.com
    /// </value>
    public string? IssuerUri
    {
        get;
        set;
    }

    /// <summary>
    /// Set to false to preserve the original casing of the IssuerUri. Defaults to true.
    /// </summary>
    public bool LowerCaseIssuerUri
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the value for the JWT typ header for access tokens.
    /// </summary>
    /// <value>
    /// The JWT typ value.
    /// </value>
    public string AccessTokenJwtType
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the max input length restrictions.
    /// </summary>
    /// <value>
    /// The length restrictions.
    /// </value>
    public InputLengthRestrictions InputLengthRestrictions
    {
        get;
        set;
    }
    
    /// <summary>
    /// Gets or sets the endpoint configuration.
    /// </summary>
    /// <value>
    /// The endpoints configuration.
    /// </value>
    public EndpointsOptions Endpoints
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the discovery endpoint configuration.
    /// </summary>
    /// <value>
    /// The discovery endpoint configuration.
    /// </value>
    public DiscoveryOptions Discovery
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the authentication options.
    /// </summary>
    /// <value>
    /// The authentication options.
    /// </value>
    public AuthenticationOptions Authentication
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the events options.
    /// </summary>
    /// <value>
    /// The events options.
    /// </value>
    public EventsOptions Events
    {
        get;
        set;
    }

    /// <summary>
    /// Specifies whether the s_hash claim gets emitted in identity tokens. Defaults to false.
    /// </summary>
    public bool EmitStateHash
    {
        get;
        set;
    }

    /// <summary>
    /// Specifies whether the JWT typ and content-type for JWT secured authorization requests is checked according to IETF spec.
    /// This might break older OIDC conformant request objects.
    /// </summary>
    public bool StrictJarValidation
    {
        get;
        set;
    }

    /// <summary>
    /// Specifies if a user's tenant claim is compared to the tenant acr_values parameter value to determine if the login page is displayed. Defaults to false.
    /// </summary>
    public bool ValidateTenantOnAuthorization
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the mutual TLS options.
    /// </summary>
    public MutualTlsOptions MutualTls
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the Content Security Policy options.
    /// </summary>
    public CspOptions Csp
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the validation options.
    /// </summary>
    public ValidationOptions Validation
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the options for the user interaction.
    /// </summary>
    /// <value>
    /// The user interaction options.
    /// </value>
    public UserInteractionOptions UserInteraction
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the caching options.
    /// </summary>
    /// <value>
    /// The caching options.
    /// </value>
    public CachingOptions Caching
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the cors options.
    /// </summary>
    /// <value>
    /// The cors options.
    /// </value>
    public CorsOptions Cors
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the signing key management options.
    /// </summary>
    public KeyManagementOptions KeyManagement
    {
        get;
        set;
    }

    /// <summary>
    /// Options for dynamic external providers.
    /// </summary>
    public DynamicProviderOptions DynamicProviders
    {
        get;
        set;
    }

    public IdentityServerOptions()
    {
        LowerCaseIssuerUri = true;
        AccessTokenJwtType = "at+jwt";
        Endpoints = new EndpointsOptions();
        Discovery = new DiscoveryOptions();
        Authentication = new AuthenticationOptions();
        Events = new EventsOptions();
        MutualTls = new MutualTlsOptions();
        Csp = new CspOptions();
        Validation = new ValidationOptions();
        UserInteraction = new UserInteractionOptions();
        Caching = new CachingOptions();
        Cors = new CorsOptions();
        KeyManagement = new KeyManagementOptions();
        InputLengthRestrictions = new InputLengthRestrictions();
        DynamicProviders = new DynamicProviderOptions();
    }
}