using System.Diagnostics;

namespace SampleBlog.IdentityServer;

internal static class Tracing
{
    private static readonly Version? AssemblyVersion;
    private static readonly string ServiceVersion;

    /// <summary>
    /// Base ActivitySource
    /// </summary>
    public static ActivitySource BasicActivitySource
    {
        get;
    }

    /// <summary>
    /// Store ActivitySource
    /// </summary>
    public static ActivitySource StoreActivitySource
    {
        get;
    }

    /// <summary>
    /// Cache ActivitySource
    /// </summary>
    public static ActivitySource CacheActivitySource
    {
        get;
    }

    /// <summary>
    /// Cache ActivitySource
    /// </summary>
    public static ActivitySource ServiceActivitySource
    {
        get;
    }

    /// <summary>
    /// Detailed validation ActivitySource
    /// </summary>
    public static ActivitySource ValidationActivitySource
    {
        get;
    }

    /// <summary>
    /// Standard ActivitySource for IdentityServer
    /// </summary>
    public static ActivitySource ActivitySource
    {
        get;
    }

    /// <summary>
    /// Service name
    /// </summary>
    public static readonly string ServiceName = "SampleBlog.IdentityServer";

    static Tracing()
    {
        var assemblyVersion = typeof(Tracing).Assembly.GetName().Version;
        var serviceVersion = assemblyVersion?.ToString(3) ?? $"{assemblyVersion?.Major}.{assemblyVersion?.Minor}.{assemblyVersion?.Build}";

        AssemblyVersion = assemblyVersion;
        ServiceVersion = serviceVersion;
        BasicActivitySource = new ActivitySource(TraceNames.Basic, serviceVersion);
        StoreActivitySource = new ActivitySource(TraceNames.Store, serviceVersion);
        CacheActivitySource = new ActivitySource(TraceNames.Cache, serviceVersion);
        ServiceActivitySource = new ActivitySource(TraceNames.Services, serviceVersion);
        ValidationActivitySource = new ActivitySource(TraceNames.Validation, serviceVersion);
        ActivitySource = new(ServiceName, serviceVersion);
    }

    public static class TraceNames
    {
        /// <summary>
        /// Service name for base traces
        /// </summary>
        public static string Basic => "SampleBlog.IdentityServer";

        /// <summary>
        /// Service name for store traces
        /// </summary>
        public static string Store => Basic + ".Stores";

        /// <summary>
        /// Service name for caching traces
        /// </summary>
        public static string Cache => Basic + ".Cache";

        /// <summary>
        /// Service name for caching traces
        /// </summary>
        public static string Services => Basic + ".Services";

        /// <summary>
        /// Service name for detailed validation traces
        /// </summary>
        public static string Validation => Basic + ".Validation";
    }

    public static class Properties
    {
        public const string EndpointType = "endpoint_type";

        public const string ClientId = "client_id";
        public const string GrantType = "grant_type";
        public const string Scope = "scope";
        public const string Resource = "resource";

        public const string Origin = "origin";
        public const string Scheme = "scheme";
        public const string Type = "type";
        public const string Id = "id";
        public const string ScopeNames = "scope_names";
        public const string ApiResourceNames = "api_resource_names";

    }
}