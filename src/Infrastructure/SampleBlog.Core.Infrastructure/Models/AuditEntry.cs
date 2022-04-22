using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace SampleBlog.Infrastructure.Models;

public enum AuditType : byte
{
    None = 0,
    Create = 1,
    Update = 2,
    Delete = 3
}

public class AuditEntry
{
    public EntityEntry Entry
    {
        get;
    }

    public string UserId
    {
        init;
        get;
    }

    public string TableName
    {
        init;
        get;
    }

    public AuditType AuditType
    {
        init;
        get;
    }

    public List<string> ChangedColumns
    {
        get;
    }

    public AuditEntry(EntityEntry entry)
    {
        ChangedColumns = new();
        Entry = entry;
    }
}