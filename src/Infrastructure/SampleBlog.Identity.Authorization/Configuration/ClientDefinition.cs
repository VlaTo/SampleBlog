namespace SampleBlog.Identity.Authorization.Configuration;

internal sealed class ClientDefinition : ServiceDefinition
{
    public string? RedirectUri
    {
        get;
        set;
    }

    public string? LogoutUri
    {
        get;
        set;
    }

    public string ClientSecret
    {
        get;
        set;
    }
}