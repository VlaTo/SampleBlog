using System.Globalization;
using Microsoft.AspNetCore.Components;

namespace SampleBlog.Web.Client.Core.Extensions;

internal static class DecimalExtensions
{
    public static MarkupString ToRubles(this decimal currency) => ToRubles(currency, CultureInfo.CurrentUICulture);
    
    public static MarkupString ToRubles(this decimal currency, IFormatProvider formatProvider)
    {
        return new MarkupString(String.Format(formatProvider, "{0:N2} &#x20BD;", currency));
    }
}