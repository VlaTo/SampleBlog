namespace SampleBlog.IdentityServer.EntityFramework.Storage.Entities;

public class ApiResourceClaim : UserClaim
{
    public int ApiResourceId
    {
        get;
        set;
    }

    public ApiResource ApiResource
    {
        get;
        set;
    }
}