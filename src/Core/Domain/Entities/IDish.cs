using SampleBlog.Core.Domain.Contracts;

namespace SampleBlog.Core.Domain.Entities;

public interface IDish : IEntity
{
    IProduct Product
    {
        get;
    }
}