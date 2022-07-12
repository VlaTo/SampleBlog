namespace SampleBlog.Web.Client.Store.OriginalMenu.Actions;

public sealed class NoOriginalMenuAction
{
    public DateTime DateTime
    {
        get;
    }

    public NoOriginalMenuAction(DateTime dateTime)
    {
        DateTime = dateTime;
    }
}