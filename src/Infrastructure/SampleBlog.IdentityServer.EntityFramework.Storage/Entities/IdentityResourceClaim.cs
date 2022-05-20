using System.ComponentModel.DataAnnotations.Schema;

namespace SampleBlog.IdentityServer.EntityFramework.Storage.Entities;

[Table(Database.Tables.IdentityResourceClaim, Schema = Database.Schemas.Identity)]
public class IdentityResourceClaim : UserClaim
{
    public int IdentityResourceId
    {
        get;
        set;
    }

    [ForeignKey(nameof(IdentityResourceId))]
    public IdentityResource IdentityResource
    {
        get;
        set;
    }
}