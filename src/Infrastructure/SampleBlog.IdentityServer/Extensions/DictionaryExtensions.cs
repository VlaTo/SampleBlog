using System.Collections.Specialized;

namespace SampleBlog.IdentityServer.Extensions;

public static class DictionaryExtensions
{
    public static NameValueCollection FromFullDictionary(this IDictionary<string, string[]?> source)
    {
        var nvc = new NameValueCollection();

        foreach (var (key, strings) in source)
        {
            if (null == strings)
            {
                continue;
            }

            foreach (var value in strings)
            {
                nvc.Add(key, value);
            }
        }

        return nvc;
    }
}