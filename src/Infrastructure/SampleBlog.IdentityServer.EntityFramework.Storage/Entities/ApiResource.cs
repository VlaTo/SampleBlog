using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SampleBlog.IdentityServer.EntityFramework.Storage.Entities;

[Table(Database.Tables.ApiResource, Schema = Database.Schemas.Identity)]
public class ApiResource
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

    public string AllowedAccessTokenSigningAlgorithms
    {
        get;
        set;
    }

    public bool ShowInDiscoveryDocument
    {
        get;
        set;
    }

    public bool RequireResourceIndicator
    {
        get;
        set;
    }

    public List<ApiResourceSecret> Secrets
    {
        get;
        set;
    }

    public List<ApiResourceScope> Scopes
    {
        get;
        set;
    }

    public List<ApiResourceClaim> UserClaims
    {
        get;
        set;
    }

    public List<ApiResourceProperty> Properties
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

    public DateTime? LastAccessed
    {
        get;
        set;
    }

    public bool NonEditable
    {
        get;
        set;
    }

    public ApiResource()
    {
        Enabled = true;
        ShowInDiscoveryDocument = true;
        //Created = DateTime.UtcNow;
    }
}