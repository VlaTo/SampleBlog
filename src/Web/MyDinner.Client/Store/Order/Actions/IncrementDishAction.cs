using SampleBlog.Web.Shared.Models.Menu;

namespace SampleBlog.Web.Client.Store.Order.Actions;

public class IncrementDishAction
{
    public Dish Entry
    {
        get;
    }

    public int Count
    {
        get;
    }

    public IncrementDishAction(Dish entry, int count)
    {
        Entry = entry;
        Count = count;
    }
}