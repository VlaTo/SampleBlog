using SampleBlog.Core.Domain.Entities;

namespace SampleBlog.Web.APi.MyDinner.Models;

public class ProductGroup : IProductGroup
{
    public string Name
    {
        get;
        init;
    }
}