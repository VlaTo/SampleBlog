namespace SampleBlog.Web.Client.Core.Configuration;

[Serializable]
public class ClientOptions
{
    public HashIdOptions HashIdOptions
    {
        get;
        set;
    }

    public ClientOptions()
    {
        HashIdOptions = new HashIdOptions();
    }
}