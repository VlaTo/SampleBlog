using SampleBlog.Web.Shared.Models.Menu;

namespace SampleBlog.Web.Client.Store.Menu.Actions;

public sealed class MenuAcquiredAction
{
    public DateTime DateTime
    {
        get;
    }

    public DishEntry[]? Dishes
    {
        get;
    }

    public MenuAcquiredAction(DateTime dateTime, DishEntry[]? dishes)
    {
        DateTime = dateTime;
        Dishes = dishes;
    }
}