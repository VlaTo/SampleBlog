using System.ComponentModel.DataAnnotations.Schema;

namespace SampleBlog.IdentityServer.EntityFramework.Storage.Entities;

[Table(Database.Tables.IdentityResourceProperty, Schema = Database.Schemas.Identity)]
public class IdentityResourceProperty : Property
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