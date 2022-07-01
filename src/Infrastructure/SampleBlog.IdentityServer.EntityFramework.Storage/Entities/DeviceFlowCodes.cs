using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SampleBlog.IdentityServer.EntityFramework.Storage.Entities;

/// <summary>
/// Entity for device flow codes
/// </summary>
[Table("DeviceCodes", Schema = Database.Schemas.Identity)]
public class DeviceFlowCodes
{
    /// <summary>
    /// Gets or sets the device code.
    /// </summary>
    /// <value>
    /// The device code.
    /// </value>
    [Required]
    [MaxLength(200)]
    [DataType("nvarchar(200)")]
    public string DeviceCode
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the user code.
    /// </summary>
    /// <value>
    /// The user code.
    /// </value>
    [Key]
    [MaxLength(200)]
    public string UserCode
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the subject identifier.
    /// </summary>
    /// <value>
    /// The subject identifier.
    /// </value>
    [MaxLength(200)]
    [DataType("nvarchar")]
    public string SubjectId
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the session identifier.
    /// </summary>
    /// <value>
    /// The session identifier.
    /// </value>
    [MaxLength(100)]
    [DataType("nvarchar")]
    public string? SessionId
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the client identifier.
    /// </summary>
    /// <value>
    /// The client identifier.
    /// </value>
    [Required]
    [MaxLength(200)]
    public string ClientId
    {
        get;
        set;
    }

    /// <summary>
    /// Gets the description the user assigned to the device being authorized.
    /// </summary>
    /// <value>
    /// The description.
    /// </value>
    [MaxLength(200)]
    public string? Description
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the creation time.
    /// </summary>
    /// <value>
    /// The creation time.
    /// </value>
    public DateTime CreationTime
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the expiration.
    /// </summary>
    /// <value>
    /// The expiration.
    /// </value>
    [Required]
    public DateTime Expiration
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the data.
    /// </summary>
    /// <value>
    /// The data.
    /// </value>
    [Required]
    [MaxLength(50000)]
    public string Data
    {
        get;
        set;
    }
}