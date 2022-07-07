namespace SampleBlog.Web.Client.Store.Menu.Actions;

public sealed class GetMenuAction
{
    public DateTime DateTime
    {
        get;
    }

    public GetMenuAction(DateTime dateTime)
    {
        DateTime = dateTime;
    }
}