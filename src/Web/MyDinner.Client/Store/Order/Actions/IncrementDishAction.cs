using SampleBlog.Web.Shared.Models.Menu;

namespace SampleBlog.Web.Client.Store.Order.Actions;

public class IncrementDishAction
{
    public DishEntry Entry
    {
        get;
    }

    public int Count
    {
        get;
    }

    public IncrementDishAction(DishEntry entry, int count)
    {
        Entry = entry;
        Count = count;
    }
}