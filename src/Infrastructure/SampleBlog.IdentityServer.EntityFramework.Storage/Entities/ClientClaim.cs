using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SampleBlog.IdentityServer.EntityFramework.Storage.Entities;

[Table(Database.Tables.ClientClaim, Schema = Database.Schemas.Identity)]
public class ClientClaim
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id
    {
        get;
        set;
    }

    public string Type
    {
        get;
        set;
    }

    public string Value
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