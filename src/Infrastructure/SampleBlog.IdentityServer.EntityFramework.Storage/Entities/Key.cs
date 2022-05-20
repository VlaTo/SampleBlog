﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SampleBlog.IdentityServer.EntityFramework.Storage.Entities;

/// <summary>
/// Models storage for keys.
/// </summary>
[Table("Keys", Schema = Database.Schemas.Identity)]
public class Key
{
    [Key]
    public string Id
    {
        get;
        set;
    }

    public int Version
    {
        get;
        set;
    }

    public DateTime Created
    {
        get;
        set;
    }

    public string Use
    {
        get;
        set;
    }

    public string Algorithm
    {
        get;
        set;
    }

    public bool IsX509Certificate
    {
        get;
        set;
    }

    public bool DataProtected
    {
        get;
        set;
    }

    public string Data
    {
        get;
        set;
    }
}