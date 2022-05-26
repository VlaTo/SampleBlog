using System.Text;

namespace SampleBlog.IdentityServer.EntityFramework.Storage.Extensions;

internal static class StringsExtensions
{
    public static string ToSpaceSeparatedString(this IEnumerable<string> strings)
    {
        return new StringBuilder()
            .AppendJoin(' ', strings)
            .ToString();
    }
}