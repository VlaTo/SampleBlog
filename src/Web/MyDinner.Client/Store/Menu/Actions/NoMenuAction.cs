namespace SampleBlog.Web.Client.Store.Menu.Actions;

public sealed class NoMenuAction
{
    public DateTime DateTime
    {
        get;
    }

    public NoMenuAction(DateTime dateTime)
    {
        DateTime = dateTime;
    }
}