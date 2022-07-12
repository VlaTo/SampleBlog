using SampleBlog.Core.Domain.Entities;

namespace SampleBlog.Web.APi.MyDinner.Models;

internal sealed class Dish : IDish
{
    public Product Product
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

    IFoodCategory? IDish.FoodCategory => Group;

    public FoodCategory? Group
    {
        get;
        init;
    }
}