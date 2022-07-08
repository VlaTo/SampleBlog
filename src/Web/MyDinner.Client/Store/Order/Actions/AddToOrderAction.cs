using SampleBlog.Web.Shared.Models.Menu;

namespace SampleBlog.Web.Client.Store.Order.Actions;

public class AddToOrderAction
{
    public DishEntry Entry
    {
        get;
    }

    public AddToOrderAction(DishEntry entry)
    {
        Entry = entry;
    }
}