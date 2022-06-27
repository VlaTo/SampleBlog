using SampleBlog.IdentityServer.Validation.Contexts;

namespace SampleBlog.IdentityServer.Validation;

/// <summary>
/// The device code validator
/// </summary>
public interface IDeviceCodeValidator
{
    /// <summary>
    /// Validates the device code.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <returns></returns>
    Task ValidateAsync(DeviceCodeValidationContext context);
}