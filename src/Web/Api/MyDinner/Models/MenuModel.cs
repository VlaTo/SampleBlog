using SampleBlog.Core.Domain.Entities;

namespace SampleBlog.Web.APi.MyDinner.Models;

internal sealed class MenuModel : IMenu
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

    public List<DishModel> Dishes
    {
        get;
        internal set;
    }

    public MenuModel()
    {
        Dishes = new List<DishModel>();
    }
}