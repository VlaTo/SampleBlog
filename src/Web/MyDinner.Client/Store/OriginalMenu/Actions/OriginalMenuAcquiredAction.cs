using SampleBlog.Web.Shared.Models.Menu;

namespace SampleBlog.Web.Client.Store.OriginalMenu.Actions;

public sealed class OriginalMenuAcquiredAction
{
    public DateTime DateTime
    {
        get;
    }

    public Dish[]? Dishes
    {
        get;
    }

    public bool IsOpen
    {
        get;
    }

    public OriginalMenuAcquiredAction(DateTime dateTime, Dish[]? dishes, bool isOpen)
    {
        DateTime = dateTime;
        Dishes = dishes;
        IsOpen = isOpen;
    }
}