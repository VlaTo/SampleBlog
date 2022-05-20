using System.Collections.Specialized;
using System.Diagnostics;
using Microsoft.Extensions.Primitives;

namespace SampleBlog.IdentityServer.Extensions;

public static class IReadableStringCollectionExtensions
{
    [DebuggerStepThrough]
    public static NameValueCollection AsNameValueCollection(this IEnumerable<KeyValuePair<string, StringValues>> collection)
    {
        var nv = new NameValueCollection();

        foreach (var field in collection)
        {
            foreach (var val in field.Value)
            {
                nv.Add(field.Key, val);
            }
        }

        return nv;
    }

    [DebuggerStepThrough]
    public static NameValueCollection AsNameValueCollection(this IDictionary<string, StringValues> collection)
    {
        var nv = new NameValueCollection();

        foreach (var field in collection)
        {
            foreach (var item in field.Value)
            {
                nv.Add(field.Key, item);
            }
        }

        return nv;
    }
}