namespace SampleBlog.IdentityServer.Storage.Extensions;

public static class PersistentGrantExtensions
{
    /// <summary>
    /// Validates the PersistedGrantFilter and throws if invalid.
    /// </summary>
    /// <param name="filter"></param>
    public static void Validate(this PersistedGrantFilter filter)
    {
        if (null == filter)
        {
            throw new ArgumentNullException(nameof(filter));
        }

        if ((String.IsNullOrWhiteSpace(filter.ClientId) || null == filter.ClientIds) &&
            String.IsNullOrWhiteSpace(filter.SessionId) &&
            String.IsNullOrWhiteSpace(filter.SubjectId) &&
            (String.IsNullOrWhiteSpace(filter.Type) || null == filter.Types))
        {
            throw new ArgumentException("No filter values set.", nameof(filter));
        }
    }
}