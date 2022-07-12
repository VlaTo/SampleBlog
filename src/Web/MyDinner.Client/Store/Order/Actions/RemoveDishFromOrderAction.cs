using SampleBlog.Web.Shared.Models.Menu;

namespace SampleBlog.Web.Client.Store.Order.Actions;

public sealed class RemoveDishFromOrderAction
{
    public Dish Entry
    {
        get;
    }

    public RemoveDishFromOrderAction(Dish entry)
    {
        Entry = entry;
    }
}