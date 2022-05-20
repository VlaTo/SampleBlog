using System.ComponentModel.DataAnnotations.Schema;

namespace SampleBlog.IdentityServer.EntityFramework.Storage.Entities;

[Table(Database.Tables.ApiScopeProperty, Schema = Database.Schemas.Identity)]
public class ApiScopeProperty : Property
{
    public int ScopeId
    {
        get;
        set;
    }

    [ForeignKey(nameof(ScopeId))]
    public ApiScope Scope
    {
        get;
        set;
    }
}