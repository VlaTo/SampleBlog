using System.Security.Cryptography;

namespace SampleBlog.IdentityServer.Extensions;

public static class HashExtensions
{
    /// <summary>
    /// Creates a SHA256 hash of the specified input.
    /// </summary>
    /// <param name="input">The input.</param>
    /// <returns>A hash.</returns>
    public static byte[]? Sha256(this byte[]? input)
    {
        if (null == input)
        {
            return null;
        }

        using (var sha = SHA256.Create())
        {
            return sha.ComputeHash(input);
        }
    }
}