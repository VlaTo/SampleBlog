namespace SampleBlog.Web.Client.Store.Menu.Actions;

public sealed class FilterMenuAction
{
    public string FoodCategories
    {
        get;
    }

    public FilterMenuAction(string foodCategories)
    {
        FoodCategories = foodCategories;
    }
}