namespace SampleBlog.Core.Domain.Contracts;

public interface IEntity
{
}

public interface IEntity<out TId> : IEntity
{
    TId Id
    {
        get;
    }
}