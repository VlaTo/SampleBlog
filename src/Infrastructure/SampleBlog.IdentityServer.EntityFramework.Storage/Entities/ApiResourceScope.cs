using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SampleBlog.IdentityServer.EntityFramework.Storage.Entities;

[Table(Database.Tables.ApiResourceScope, Schema = Database.Schemas.Identity)]
public class ApiResourceScope
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id
    {
        get;
        set;
    }

    public string Scope
    {
        get;
        set;
    }

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