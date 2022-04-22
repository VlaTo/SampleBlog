namespace SampleBlog.Core.Application.Responses.Identity;

public sealed class TokenResponse
{
    public string Token
    {
        init;
        get;
    }

    public string RefreshToken
    {
        init;
        get;
    }
}