namespace SampleBlog.Core.Domain.Contracts;

public interface IAuditableEntity : IEntity
{
    DateTime Created
    {
        get;
        set;
    }

    DateTime Modified
    {
        get;
        set;
    }
}

public interface IAuditableEntity<out TId> : IAuditableEntity, IEntity<TId>
{
}