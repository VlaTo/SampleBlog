using System.Collections.Specialized;
using System.Diagnostics;
using Microsoft.Extensions.Primitives;

namespace SampleBlog.Identity.Authorization.Extensions;

public static class QueryCollectionExtensions
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
}