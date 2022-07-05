using System.Text;

namespace SampleBlog.Web.Client.Extensions;

internal static class StringBuilderExtensions
{
    public static StringBuilder AppendIf(this StringBuilder builder, string line, bool condition)
    {
        if (condition)
        {
            builder.Append(line);
        }

        return builder;
    }

    public static StringBuilder AppendIf(this StringBuilder builder, string line, Func<bool> condition)
    {
        if (condition.Invoke())
        {
            builder.Append(line);
        }

        return builder;
    }
}