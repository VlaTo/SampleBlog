﻿namespace SampleBlog.Core.Application.Configuration;

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
    
    public bool AllowUserLockOut
    {
        get;
        set;
    }

    public TimeSpan RememberMeSignInDuration
    {
        get;
        set;
    }

    public TimeSpan RefreshTokenDuration
    {
        get;
        set;
    }

    public TimeSpan SecurityTokenDuration
    {
        get;
        set;
    }
}