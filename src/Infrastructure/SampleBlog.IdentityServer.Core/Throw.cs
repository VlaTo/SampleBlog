using System.Diagnostics.CodeAnalysis;

namespace SampleBlog.IdentityServer.Core;

public static class Throw
{
    public static void IfNull<T>([MaybeNull]T? value, string propertyName)
    {
        if (null == value)
        {
            throw new ArgumentNullException(propertyName);
        }
    }

    public static void IfNullOrEmpty([MaybeNull]string? value, string propertyName)
    {
        if (String.IsNullOrEmpty(value))
        {
            throw new ArgumentNullException(propertyName);
        }
    }

    public static void IfNullOrEmpty<T>([MaybeNull]IEnumerable<T>? values, string propertyName)
    {
        if (null == values || false == values.Any())
        {
            throw new ArgumentNullException(propertyName);
        }
    }

    public static void IfEmpty(Array array, string propertyName)
    {
        if (0 == array.Length)
        {
            throw new ArgumentNullException(propertyName);
        }
    }
}