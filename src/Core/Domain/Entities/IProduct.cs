using SampleBlog.Core.Domain.Contracts;

namespace SampleBlog.Core.Domain.Entities;

public interface IProduct : IEntity<long>
{
    string Name
    {
        get;
    }
}