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
        var menu = new Menu
        {
            Id = 1,
            Date = dateTime,
            IsOpen = true,
            Dishes = new List<Dish>
            {
                new()
                {
                    IsEnabled = true,
                    Product = new Product
                    {
                        Id = 2,
                        Name = "Sample product #2"
                    },
                    Price = 24.45m,
                    Outcome = new Outcome(150.0f, Units.Grams),
                    Calories = 230.30f,
                    Group = new FoodCategory
                    {
                        Key = "group-1",
                        Name = "Group #1"
                    }
                },
                new()
                {
                    IsEnabled = true,
                    Product = new Product
                    {
                        Id = 3,
                        Name = "Sample product #3"
                    },
                    Price = 110.00m,
                    Outcome = new Outcome(125.0f, Units.Grams),
                    Calories = 130.25f,
                    Group = new FoodCategory
                    {
                        Key = "group-2",
                        Name = "Group #2"
                    }
                },
                new()
                {
                    IsEnabled = false,
                    Product = new Product
                    {
                        Id = 1,
                        Name = "Sample product #1"
                    },
                    Price = 150.0m,
                    Calories = 550.75f,
                    Outcome = new Outcome(1.0f, Units.Pieces),
                    Group = null
                },
                new()
                {
                    IsEnabled = true,
                    Product = new Product
                    {
                        Id = 5,
                        Name = "Sample product #5"
                    },
                    Price = 93.20m,
                    Outcome = new Outcome(90.0f, Units.Grams),
                    Calories = 345.50f,
                    Group = new FoodCategory
                    {
                        Key = "group-1",
                        Name = "Group #1"
                    }
                },
                new()
                {
                    IsEnabled = true,
                    Product = new Product
                    {
                        Id = 4,
                        Name = "Sample product #4"
                    },
                    Price = 150.0m,
                    Outcome = new Outcome(170.0f, Units.Grams),
                    Calories = 768.0f,
                    Group = new FoodCategory
                    {
                        Key = "group-1",
                        Name = "Group #1"
                    }
                },
            }
        };

        return Task.FromResult<IMenu?>(menu);
    }
}