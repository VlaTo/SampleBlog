using SampleBlog.IdentityServer.Validation.Requests;

namespace SampleBlog.IdentityServer.Validation.Results;

/// <summary>
/// Validation result for token requests
/// </summary>
public sealed class TokenRequestValidationResult : ValidationResult
{
    /// <summary>
    /// Gets the validated request.
    /// </summary>
    /// <value>
    /// The validated request.
    /// </value>
    public ValidatedTokenRequest ValidatedRequest
    {
        get;
        init;
    }

    /// <summary>
    /// Gets or sets the custom response.
    /// </summary>
    /// <value>
    /// The custom response.
    /// </value>
    public Dictionary<string, object>? CustomResponse
    {
        get;
        set;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TokenRequestValidationResult"/> class.
    /// </summary>
    /// <param name="validatedRequest">The validated request.</param>
    /// <param name="customResponse">The custom response.</param>
    public TokenRequestValidationResult(
        ValidatedTokenRequest validatedRequest,
        Dictionary<string, object>? customResponse = null)
    {
        IsError = false;

        ValidatedRequest = validatedRequest;
        CustomResponse = customResponse;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TokenRequestValidationResult"/> class.
    /// </summary>
    /// <param name="validatedRequest">The validated request.</param>
    /// <param name="error">The error.</param>
    /// <param name="errorDescription">The error description.</param>
    /// <param name="customResponse">The custom response.</param>
    public TokenRequestValidationResult(
        ValidatedTokenRequest validatedRequest,
        string error,
        string? errorDescription = null,
        Dictionary<string, object>? customResponse = null)
    {
        ValidatedRequest = validatedRequest;
        IsError = true;
        Error = error;
        ErrorDescription = errorDescription;
        CustomResponse = customResponse;
    }
}