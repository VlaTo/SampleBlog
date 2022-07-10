using SampleBlog.Web.Shared.Models.Menu;

namespace SampleBlog.Web.Client.Store.Order.Actions;

public class DecrementDishAction
{
    public DishEntry Entry
    {
        get;
    }

    public int Count
    {
        get;
    }

    public DecrementDishAction(DishEntry entry, int count)
    {
        Entry = entry;
        Count = count;
    }
}