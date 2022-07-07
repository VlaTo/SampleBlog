﻿using System.ComponentModel;
using System.Runtime.Serialization;

namespace SampleBlog.Web.APi.MyDinner.Configuration;

[Serializable]
[DataContract]
public class MyDinnerOptions
{
    [DataMember(Name = nameof(HashId))]
    public HashIdOptions HashId
    {
        get;
        set;
    }
}

[Serializable]
[DataContract]
public sealed class HashIdOptions
{
    [DataMember(Name = nameof(Salt))]
    public string Salt
    {
        get;
        set;
    }

    [DataMember(Name = nameof(MinHashLength))]
    [DefaultValue(11)]
    public int MinHashLength
    {
        get;
        set;
    }
}
