using SampleBlog.Core.Domain.Entities;

namespace SampleBlog.Web.APi.MyDinner.Models;

public class FoodCategory : IFoodCategory
{
    public string Key
    {
        get;
        init;
    }

    public string Name
    {
        get;
        init;
    }
}