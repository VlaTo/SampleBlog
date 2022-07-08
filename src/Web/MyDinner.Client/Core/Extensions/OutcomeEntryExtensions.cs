using SampleBlog.Web.Shared.Models.Menu;

namespace SampleBlog.Web.Client.Core.Extensions;

internal static class OutcomeEntryExtensions
{
    public static string? ToString(this OutcomeEntry entry, string? format, IFormatProvider formatProvider)
    {
        switch (format)
        {
            case "G":
            {
                return String.Format(formatProvider, "{0:N} {1}", entry.Amount, entry.Units);
            }
        }

        return String.Empty;
    }
}