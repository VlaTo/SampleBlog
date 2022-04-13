using System.Diagnostics;

namespace SampleBlog.IdentityServer;

internal static class Tracing
{
    //private static readonly Version? AssemblyVersion = typeof(Tracing).Assembly.GetName().Version;
    //private static string ServiceVersion => $"{AssemblyVersion?.Major}.{AssemblyVersion?.Minor}.{AssemblyVersion?.Build}";

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

    static Tracing()
    {
        var assemblyVersion = typeof(Tracing).Assembly.GetName().Version;
        var serviceVersion = assemblyVersion?.ToString(3) ?? ""; // $"{assemblyVersion?.Major}.{assemblyVersion?.Minor}.{assemblyVersion?.Build}";

        BasicActivitySource = new ActivitySource(TraceNames.Basic, serviceVersion);
        StoreActivitySource = new ActivitySource(TraceNames.Store, serviceVersion);
        CacheActivitySource = new ActivitySource(TraceNames.Cache, serviceVersion);
        ServiceActivitySource = new ActivitySource(TraceNames.Services, serviceVersion);
        ValidationActivitySource = new ActivitySource(TraceNames.Validation, serviceVersion);
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
}