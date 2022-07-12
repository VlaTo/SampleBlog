using SampleBlog.Core.Domain.Entities;

namespace SampleBlog.Web.APi.MyDinner.Models;

internal sealed class Product : IProduct
{
    public long Id
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