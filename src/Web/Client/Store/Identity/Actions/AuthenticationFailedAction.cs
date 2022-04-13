namespace SampleBlog.Web.Client.Store.Identity.Actions;

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