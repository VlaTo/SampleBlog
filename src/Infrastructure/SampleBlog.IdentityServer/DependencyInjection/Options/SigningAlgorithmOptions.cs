namespace SampleBlog.IdentityServer.DependencyInjection.Options;

/// <summary>
/// Class to configure signing algorithm.
/// </summary>
public class SigningAlgorithmOptions
{
    /// <summary>
    /// The algorithm name.
    /// </summary>
    public string Name
    {
        get;
        set;
    }

    /// <summary>
    /// Indicates if a X509 certificate is to be used to contain the key. Defaults to false.
    /// </summary>
    public bool UseX509Certificate
    {
        get;
        set;
    }

    internal bool IsRsaKey => Name.StartsWith("R") || Name.StartsWith("P");

    internal bool IsEcKey => Name.StartsWith("E");

    /// <summary>
    /// Constructor.
    /// </summary>
    public SigningAlgorithmOptions(string name)
    {
        Name = name;
    }
}