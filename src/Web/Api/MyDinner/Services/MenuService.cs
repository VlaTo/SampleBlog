using SampleBlog.Core.Domain.Entities;
using SampleBlog.Core.Domain.Services;
using SampleBlog.Infrastructure.Database.Contexts;
using SampleBlog.Web.APi.MyDinner.Models;

namespace SampleBlog.Web.APi.MyDinner.Services;

internal sealed class MenuService : IMenuService
{
    private readonly BlogContext context;

    public MenuService(BlogContext context)
    {
        this.context = context;
    }

    public Task<IMenu?> GetMenuAsync(DateTime dateTime, CancellationToken cancellationToken = default)
    {
        var dishes = new List<IDish>
        {
            new DishModel
            {
                Product = new ProductModel
                {
                    Id = 2,
                    Name = "Sample product"
                }
            }
        };

        var menu = new MenuModel
        {
            Id = 1,
            Date = dateTime,
            Dishes = dishes
        };

        return Task.FromResult<IMenu?>(menu);
    }
}