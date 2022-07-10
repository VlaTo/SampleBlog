using SampleBlog.Web.Shared.Models.Menu;

namespace SampleBlog.Web.Client.Store.Order.Actions;

public sealed class RemoveDishFromOrderAction
{
    public DishEntry Entry
    {
        get;
    }

    public RemoveDishFromOrderAction(DishEntry entry)
    {
        Entry = entry;
    }
}