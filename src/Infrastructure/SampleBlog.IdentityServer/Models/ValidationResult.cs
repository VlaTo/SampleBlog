namespace SampleBlog.IdentityServer.Models;

/// <summary>
/// Minimal validation result class (base-class for more complex validation results)
/// </summary>
public class ValidationResult
{
    /// <summary>
    /// Gets or sets a value indicating whether the validation was successful.
    /// </summary>
    /// <value>
    ///   <c>true</c> if the validation is failed; otherwise, <c>false</c>.
    /// </value>
    public bool IsError
    {
        get;
    }

    /// <summary>
    /// Gets or sets the error.
    /// </summary>
    /// <value>
    /// The error.
    /// </value>
    public string? Error
    {
        get;
    }

    /// <summary>
    /// Gets or sets the error description.
    /// </summary>
    /// <value>
    /// The error description.
    /// </value>
    public string? ErrorDescription
    {
        get;
        set;
    }

    public ValidationResult(bool isError, string? error = null, string? errorDescription = null)
    {
        IsError = isError;
        Error = error;
        ErrorDescription = errorDescription;
    }
}