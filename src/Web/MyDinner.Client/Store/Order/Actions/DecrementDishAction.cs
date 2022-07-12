using SampleBlog.Web.Shared.Models.Menu;

namespace SampleBlog.Web.Client.Store.Order.Actions;

public class DecrementDishAction
{
    public Dish Entry
    {
        get;
    }

    public int Count
    {
        get;
    }

    public DecrementDishAction(Dish entry, int count)
    {
        Entry = entry;
        Count = count;
    }
}