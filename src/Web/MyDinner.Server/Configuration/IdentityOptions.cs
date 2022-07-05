using System.Runtime.Serialization;

namespace SampleBlog.Web.Application.MyDinner.Server.Configuration;


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