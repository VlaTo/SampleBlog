namespace SampleBlog.Core.Application.Configuration;

public sealed class ApplicationOptions
{
    public AuthenticationOptions Authentication
    {
        get;
        set;
    }
}

public sealed class AuthenticationOptions
{
    public string Secret
    {
        get;
        set;
    }

    public bool AllowRememberMe
    {
        get;
        set;
    }

    public TimeSpan RememberMeSignInDuration
    {
        get;
        set;
    }
}