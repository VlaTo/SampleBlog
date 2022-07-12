using SampleBlog.Core.Domain.Contracts;

namespace SampleBlog.Core.Domain.Entities;

public interface IDish : IEntity
{
    IProduct Product
    {
        get;
    }

    bool IsEnabled
    {
        get;
    }

    Outcome Outcome
    {
        get;
    }

    float Calories
    {
        get;
    }

    decimal Price
    {
        get;
    }

    IFoodCategory? FoodCategory
    {
        get;
    }
}