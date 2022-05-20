using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SampleBlog.IdentityServer.EntityFramework.Storage.Entities;

[Table(Database.Tables.ClientRedirectUri, Schema = Database.Schemas.Identity)]
public class ClientRedirectUri
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id
    {
        get;
        set;
    }

    public string RedirectUri
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