using System.Collections.Specialized;
using System.Security.Claims;
using SampleBlog.IdentityServer.Models;

namespace SampleBlog.IdentityServer.Services;

/// <summary>
///  Authorize endpoint request validator.
/// </summary>
public interface IAuthorizeRequestValidator
{
    /// <summary>
    ///  Validates authorize request parameters.
    /// </summary>
    /// <param name="parameters"></param>
    /// <param name="subject"></param>
    /// <returns></returns>
    Task<AuthorizeRequestValidationResult> ValidateAsync(NameValueCollection parameters, ClaimsPrincipal? subject = null);
}