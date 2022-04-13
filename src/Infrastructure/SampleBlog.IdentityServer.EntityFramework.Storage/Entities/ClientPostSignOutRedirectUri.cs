namespace SampleBlog.IdentityServer.EntityFramework.Storage.Entities;

public class ClientPostSignOutRedirectUri
{
    public int Id
    {
        get;
        set;
    }

    public string PostLogoutRedirectUri
    {
        get;
        set;
    }

    public int ClientId
    {
        get;
        set;
    }

    public Client Client
    {
        get;
        set;
    }
}