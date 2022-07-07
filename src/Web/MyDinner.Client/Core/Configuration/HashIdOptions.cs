namespace SampleBlog.Web.Client.Core.Configuration;

public class HashIdOptions
{
    public string Salt
    {
        get;
        set;
    }

    public int MinHashLength
    {
        get;
        set;
    }
}