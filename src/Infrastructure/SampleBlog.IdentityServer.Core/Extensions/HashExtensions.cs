using System.Security.Cryptography;
using System.Text;

namespace SampleBlog.IdentityServer.Core.Extensions;

public static class HashExtensions
{
    /// <summary>
    /// Creates a SHA256 hash of the specified input.
    /// </summary>
    /// <param name="input">The input.</param>
    /// <returns>A hash</returns>
    public static string Sha256(this string? input)
    {
        if (String.IsNullOrEmpty(input))
        {
            return String.Empty;
        }

        using (var algorithm = SHA256.Create())
        {
            var bytes = Encoding.UTF8.GetBytes(input);
            var hash = algorithm.ComputeHash(bytes);

            return Convert.ToBase64String(hash);
        }
    }
}