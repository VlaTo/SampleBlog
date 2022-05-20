using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SampleBlog.IdentityServer.EntityFramework.Storage.Entities;

[Table(Database.Tables.ClientIdPRestriction, Schema = Database.Schemas.Identity)]
public class ClientIdPRestriction
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id
    {
        get;
        set;
    }

    public string Provider
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