using System.ComponentModel.DataAnnotations.Schema;

namespace SampleBlog.IdentityServer.EntityFramework.Storage.Entities;

[Table(Database.Tables.ApiResourceProperty, Schema = Database.Schemas.Identity)]
public class ApiResourceProperty : Property
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