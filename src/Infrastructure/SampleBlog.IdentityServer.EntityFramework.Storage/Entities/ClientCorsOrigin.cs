using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SampleBlog.IdentityServer.EntityFramework.Storage.Entities;

[Table(Database.Tables.ClientCorsOrigin, Schema = Database.Schemas.Identity)]
public class ClientCorsOrigin
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id
    {
        get;
        set;
    }

    public string Origin
    {
        get;
        set;
    }

    public int ClientId
    {
        get;
        set;
    }

    [ForeignKey(nameof(ClientId))]
    public Client Client
    {
        get;
        set;
    }
}