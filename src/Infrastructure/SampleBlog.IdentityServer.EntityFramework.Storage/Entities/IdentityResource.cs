using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SampleBlog.IdentityServer.EntityFramework.Storage.Entities;

[Table(Database.Tables.IdentityResource, Schema = Database.Schemas.Identity)]
public class IdentityResource
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id
    {
        get;
        set;
    }

    public bool Enabled
    {
        get;
        set;
    }

    public string Name
    {
        get;
        set;
    }

    public string DisplayName
    {
        get;
        set;
    }

    public string Description
    {
        get;
        set;
    }

    public bool Required
    {
        get;
        set;
    }

    public bool Emphasize
    {
        get;
        set;
    }

    public bool ShowInDiscoveryDocument
    {
        get;
        set;
    }

    public List<IdentityResourceClaim> UserClaims
    {
        get;
        set;
    }

    public List<IdentityResourceProperty> Properties
    {
        get;
        set;
    }

    public DateTime Created
    {
        get;
        set;
    }

    public DateTime? Updated
    {
        get;
        set;
    }

    public bool NonEditable
    {
        get;
        set;
    }

    public IdentityResource()
    {
        Enabled = true;
        ShowInDiscoveryDocument = true;
        UserClaims = new List<IdentityResourceClaim>();
        Properties = new List<IdentityResourceProperty>();
        //Created = DateTime.UtcNow;
    }
}