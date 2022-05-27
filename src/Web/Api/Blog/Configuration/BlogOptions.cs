using System.ComponentModel;
using System.Runtime.Serialization;

namespace SampleBlog.Web.APi.Blog.Configuration;

[Serializable]
[DataContract]
public class BlogOptions
{
    [DataMember(Name = nameof(HashId))]
    public HashIdOptions HashId
    {
        get;
        set;
    }
}

[Serializable]
[DataContract]
public sealed class HashIdOptions
{
    [DataMember(Name = nameof(Salt))]
    public string Salt
    {
        get;
        set;
    }

    [DataMember(Name = nameof(MinHashLength))]
    [DefaultValue(11)]
    public int MinHashLength
    {
        get;
        set;
    }
}
