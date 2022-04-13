﻿namespace SampleBlog.IdentityServer.EntityFramework.Storage.Entities;

/// <summary>
/// Models storage for keys.
/// </summary>
public class Key
{
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