using System.Runtime.Serialization;

namespace SampleBlog.Web.Identity.Configuration;


[Serializable, DataContract]
public sealed class IdentityOptions
{
    [DataMember(Name = nameof(Secret))]
    public string Secret
    {
        get;
        set;
    }
}