using SampleBlog.IdentityServer.Models;
using SampleBlog.IdentityServer.ResponseHandling.Models;
using SampleBlog.IdentityServer.Validation.Requests;

namespace SampleBlog.IdentityServer.ResponseHandling;

/// <summary>
/// Interface for determining if user must login or consent when making requests to the authorization endpoint.
/// </summary>
public interface IAuthorizeInteractionResponseGenerator
{
    /// <summary>
    /// Processes the interaction logic.
    /// </summary>
    /// <param name="request">The request.</param>
    /// <param name="consent">The consent.</param>
    /// <returns></returns>
    Task<InteractionResponse> ProcessInteractionAsync(ValidatedAuthorizeRequest request, ConsentResponse? consent = null);
}
