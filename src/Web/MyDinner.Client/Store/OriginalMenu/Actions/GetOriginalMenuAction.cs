namespace SampleBlog.Web.Client.Store.OriginalMenu.Actions;

public sealed class GetOriginalMenuAction
{
    public DateTime DateTime
    {
        get;
    }

    public GetOriginalMenuAction(DateTime dateTime)
    {
        DateTime = dateTime;
    }
}