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
        var menu = new MenuModel
        {
            Id = 1,
            Date = dateTime,
            IsOpen = true,
            Dishes = new List<DishModel>
            {
                new()
                {
                    IsEnabled = true,
                    Product = new ProductModel
                    {
                        Id = 2,
                        Name = "Sample product #2"
                    },
                    Price = 24.45m,
                    Outcome = new Outcome(150.0f, Units.Grams),
                    Group = new ProductGroup
                    {
                        Name = "Group #1"
                    }
                },
                new()
                {
                    IsEnabled = true,
                    Product = new ProductModel
                    {
                        Id = 3,
                        Name = "Sample product #3"
                    },
                    Price = 110.00m,
                    Outcome = new Outcome(125.0f, Units.Grams),
                    Group = new ProductGroup
                    {
                        Name = "Group #2"
                    }
                },
                new()
                {
                    IsEnabled = false,
                    Product = new ProductModel
                    {
                        Id = 1,
                        Name = "Sample product #1"
                    },
                    Price = 150.0m,
                    Outcome = new Outcome(1.0f, Units.Pieces),
                    Group = null
                }
            }
        };

        return Task.FromResult<IMenu?>(menu);
    }
}