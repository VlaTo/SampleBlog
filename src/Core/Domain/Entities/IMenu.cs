using SampleBlog.Core.Domain.Contracts;

namespace SampleBlog.Core.Domain.Entities;

public interface IMenu : IEntity<long>
{
    DateTime Date
    {
        get;
    }

    IReadOnlyList<IDish> Dishes
    {
        get;
    }
}