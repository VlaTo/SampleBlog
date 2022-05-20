using System.ComponentModel.DataAnnotations.Schema;

namespace SampleBlog.IdentityServer.EntityFramework.Storage.Entities;

[Table(Database.Tables.ApiResourceSecret, Schema = Database.Schemas.Identity)]
public class ApiResourceSecret : Secret
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