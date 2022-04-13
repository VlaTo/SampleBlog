using SampleBlog.IdentityServer.Models;
using SampleBlog.IdentityServer.Validation.Requests;

namespace SampleBlog.IdentityServer.Validation;

/// <summary>
/// Validates requested resources (scopes and resource indicators)
/// </summary>
public interface IResourceValidator
{
    /// <summary>
    /// Validates the requested resources for the client.
    /// </summary>
    Task<ResourceValidationResult> ValidateRequestedResourcesAsync(ResourceValidationRequest request);
}