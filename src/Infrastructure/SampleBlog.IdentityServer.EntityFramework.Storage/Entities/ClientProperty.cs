using System.ComponentModel.DataAnnotations.Schema;

namespace SampleBlog.IdentityServer.EntityFramework.Storage.Entities;

[Table(Database.Tables.ClientProperty, Schema = Database.Schemas.Identity)]
public class ClientProperty : Property
{
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