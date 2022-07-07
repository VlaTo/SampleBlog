using SampleBlog.Core.Domain.Entities;

namespace SampleBlog.Web.APi.MyDinner.Models;

internal sealed class DishModel : IDish
{
    public IProduct Product
    {
        get;
        init;
    }
}