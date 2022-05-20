using System.ComponentModel.DataAnnotations.Schema;

namespace SampleBlog.IdentityServer.EntityFramework.Storage.Entities;

[Table(Database.Tables.ApiScopeClaim, Schema = Database.Schemas.Identity)]
public class ApiScopeClaim : UserClaim
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