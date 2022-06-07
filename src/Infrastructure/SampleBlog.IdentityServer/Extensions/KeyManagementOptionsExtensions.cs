using SampleBlog.IdentityServer.DependencyInjection.Options;

namespace SampleBlog.IdentityServer.Extensions;

internal static class KeyManagementOptionsExtensions
{
    public static bool IsWithinInitializationDuration(this KeyManagementOptions options, TimeSpan age)
    {
        return (age <= options.InitializationDuration);
    }

    public static bool IsRetired(this KeyManagementOptions options, TimeSpan age)
    {
        return (age >= options.KeyRetirementAge);
    }

    public static bool IsExpired(this KeyManagementOptions options, TimeSpan age)
    {
        return (age >= options.RotationInterval);
    }
}