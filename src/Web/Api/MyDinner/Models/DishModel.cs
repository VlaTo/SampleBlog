using SampleBlog.Core.Domain.Entities;

namespace SampleBlog.Web.APi.MyDinner.Models;

internal sealed class DishModel : IDish
{
    public ProductModel Product
    {
        get;
        init;
    }

    IProduct IDish.Product => Product;

    public bool IsEnabled
    {
        get;
        init;
    }

    public Outcome Outcome
    {
        get;
        init;
    }

    public float Calories
    {
        get;
        init;
    }

    public decimal Price
    {
        get;
        init;
    }

    IProductGroup? IDish.Group => Group;

    public ProductGroup? Group
    {
        get;
        init;
    }
}