﻿namespace SampleBlog.IdentityServer.Validation;

/// <summary>
/// Compares resource_type strings, where the order of space-delimited values is insignificant.
/// </summary>
/// <remarks>
/// <para>
/// This is to handle the fact that the order of multi-valued response_type lists is
/// insignificant, per the <see href="https://tools.ietf.org/html/rfc6749#section-3.1.1">OAuth2 spec</see>
/// and the 
/// (<see href="http://openid.net/specs/oauth-v2-multiple-response-types-1_0-03.html#terminology">OAuth 
/// 2.0 Multiple Response Type Encoding Practices draft </see>).
/// </para>
/// </remarks>
public class ResponseTypeEqualityComparer : IEqualityComparer<string>
{
    private const char separator = ' ';

    /// <summary>
    /// Determines whether the specified values are equal.
    /// </summary>
    /// <param name="x">The first string to compare.</param>
    /// <param name="y">The second string to compare.</param>
    /// <returns>true if the specified values are equal; otherwise, false.</returns>
    public bool Equals(string? x, string? y)
    {
        if (x == y)
        {
            return true;
        }

        if (x == null || y == null)
        {
            return false;
        }

        if (x.Length != y.Length)
        {
            return false;
        }

        var xValues = x.Split(separator);
        var yValues = y.Split(separator);

        if (xValues.Length != yValues.Length)
        {
            return false;
        }

        Array.Sort(xValues);
        Array.Sort(yValues);

        for (var index = 0; xValues.Length > index; index++)
        {
            if (xValues[index] != yValues[index])
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Returns a hash code for the value.
    /// </summary>
    /// <param name="value">The value for which a hash code is to be returned.</param>
    /// <returns>A hash code for the value, suitable for use in hashing algorithms and data structures like a hash table.</returns>
    public int GetHashCode(string value)
    {
        var values = value.Split(separator);

        if (1 == values.Length)
        {
            // Only one value, so just spit out the hash code of the whole string
            return value.GetHashCode();
        }

        Array.Sort(values);

        // Using Skeet's answer here: http://stackoverflow.com/a/7244729/208990
        // Licensed under Creative Commons CC-BY-SA from SO: https://stackoverflow.com/legal/terms-of-service#licensing
        // Creative Commons CC-BY-SA https://creativecommons.org/licenses/by-sa/4.0/
        var comparer = StringComparer.Ordinal;
        var hash = 17;

        foreach (var element in values)
        {
            hash = hash * 31 + comparer.GetHashCode(element);
        }

        return hash;
    }
}