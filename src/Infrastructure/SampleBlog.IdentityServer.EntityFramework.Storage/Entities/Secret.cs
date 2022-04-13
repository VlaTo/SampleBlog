using SampleBlog.IdentityServer.Storage;

namespace SampleBlog.IdentityServer.EntityFramework.Storage.Entities;

public abstract class Secret
{
    public int Id
    {
        get;
        set;
    }

    public string Description
    {
        get;
        set;
    }

    public string Value
    {
        get;
        set;
    }

    public DateTime? Expiration
    {
        get;
        set;
    }

    public string Type
    {
        get;
        set;
    }

    public DateTime Created
    {
        get;
        set;
    }

    protected Secret()
    {
        Type = IdentityServerConstants.SecretTypes.SharedSecret;
        Created = DateTime.UtcNow;
    }
}