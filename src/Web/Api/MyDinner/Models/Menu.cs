using SampleBlog.Core.Domain.Entities;

namespace SampleBlog.Web.APi.MyDinner.Models;

internal sealed class Menu : IMenu
{
    public long Id
    {
        get;
        init;
    }

    public DateTime Date
    {
        get;
        init;
    }

    public bool IsOpen
    {
        get;
        init;
    }

    IReadOnlyList<IDish> IMenu.Dishes => Dishes;

    public List<Dish> Dishes
    {
        get;
        internal set;
    }

    public Menu()
    {
        Dishes = new List<Dish>();
    }
}