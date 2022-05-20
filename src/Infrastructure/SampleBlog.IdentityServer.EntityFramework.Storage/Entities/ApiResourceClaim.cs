using System.ComponentModel.DataAnnotations.Schema;

namespace SampleBlog.IdentityServer.EntityFramework.Storage.Entities;

[Table(Database.Tables.ApiResourceClaim, Schema = Database.Schemas.Identity)]
public class ApiResourceClaim : UserClaim
{
    public int ApiResourceId
    {
        get;
        set;
    }

    [ForeignKey(nameof(ApiResourceId))]
    public ApiResource ApiResource
    {
        get;
        set;
    }
}