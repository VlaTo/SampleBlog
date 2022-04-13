namespace SampleBlog.IdentityServer.EntityFramework.Storage.Entities;

public class ClientSecret : Secret
{
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