namespace SampleBlog.Core.Application.Requests.Identity;

public class SignInRequest
{
    public string Email
    {
        init;
        get;
    }

    public string Password
    {
        init;
        get;
    }

    public SignInRequest(string email, string password)
    {
        Email = email;
        Password = password;
    }
}