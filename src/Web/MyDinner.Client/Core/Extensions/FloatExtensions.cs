using Microsoft.AspNetCore.Components;

namespace SampleBlog.Web.Client.Core.Extensions;

internal static class FloatExtensions
{
    public static MarkupString ToCalories(this float value, IFormatProvider formatProvider)
    {
        return new MarkupString(String.Format(formatProvider, "{0:N0} ккал.", value));
    }
}