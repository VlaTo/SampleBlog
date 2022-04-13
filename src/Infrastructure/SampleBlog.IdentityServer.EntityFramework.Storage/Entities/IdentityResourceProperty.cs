namespace SampleBlog.IdentityServer.EntityFramework.Storage.Entities;

public class IdentityResourceProperty : Property
{
    public int IdentityResourceId
    {
        get;
        set;
    }

    public IdentityResource IdentityResource
    {
        get;
        set;
    }
}