using System.Collections.Specialized;
using System.Text;
using System.Text.Encodings.Web;

namespace SampleBlog.IdentityServer.Extensions;

internal static class NameValueCollectionExtensions
{
    public static Dictionary<string, string[]?> ToFullDictionary(this NameValueCollection? source)
    {
        var dictionary = new Dictionary<string, string[]?>();

        if (null != source)
        {
            var keys = source.AllKeys;

            for (var index = 0; index < keys.Length; index++)
            {
                var key = keys[index];

                if (String.IsNullOrEmpty(key))
                {
                    continue;
                }

                dictionary[key] = source.GetValues(key);
            }
        }

        return dictionary;
    }

    public static string ToQueryString(this NameValueCollection collection)
    {
        if (collection.Count == 0)
        {
            return String.Empty;
        }

        var builder = new StringBuilder(128);
        var first = true;

        foreach (string name in collection)
        {
            var values = collection.GetValues(name);
            if (values == null || values.Length == 0)
            {
                first = AppendNameValuePair(builder, first, true, name, String.Empty);
            }
            else
            {
                foreach (var value in values)
                {
                    first = AppendNameValuePair(builder, first, true, name, value);
                }
            }
        }

        return builder.ToString();
    }

    public static string ToFormPost(this NameValueCollection collection)
    {
        var builder = new StringBuilder(128);
        const string inputFieldFormat = "<input type='hidden' name='{0}' value='{1}' />\n";

        foreach (string name in collection)
        {
            var values = collection.GetValues(name);
            var value = values.First();

            value = HtmlEncoder.Default.Encode(value);
            builder.AppendFormat(inputFieldFormat, name, value);
        }

        return builder.ToString();
    }

    internal static string? ConvertFormUrlEncodedSpacesToUrlEncodedSpaces(string? str)
    {
        if ((null != str) && 0 <= (str.IndexOf('+')))
        {
            str = str.Replace("+", "%20");
        }

        return str;
    }

    private static bool AppendNameValuePair(StringBuilder builder, bool first, bool urlEncode, string name, string value)
    {
        var effectiveName = name ?? String.Empty;
        var effectiveValue = value ?? String.Empty;
        var encodedName = urlEncode ? UrlEncoder.Default.Encode(effectiveName) : effectiveName;
        var encodedValue = urlEncode ? UrlEncoder.Default.Encode(effectiveValue) : effectiveValue;
        
        encodedValue = ConvertFormUrlEncodedSpacesToUrlEncodedSpaces(encodedValue);

        if (first)
        {
            first = false;
        }
        else
        {
            builder.Append("&");
        }

        builder.Append(encodedName);

        if (!String.IsNullOrEmpty(encodedValue))
        {
            builder.Append("=");
            builder.Append(encodedValue);
        }

        return first;
    }
}