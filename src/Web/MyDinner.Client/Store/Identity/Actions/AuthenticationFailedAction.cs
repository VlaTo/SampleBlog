namespace SampleBlog.Web.Application.MyDinner.Client.Store.Identity.Actions;

public class AuthenticationFailedAction
{
    public Exception Exception
    {
        get;
    }

    public AuthenticationFailedAction(Exception exception)
    {
        Exception = exception;
    }
}