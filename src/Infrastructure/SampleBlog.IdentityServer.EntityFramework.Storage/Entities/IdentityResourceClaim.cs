namespace SampleBlog.IdentityServer.EntityFramework.Storage.Entities;

public class IdentityResourceClaim : UserClaim
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