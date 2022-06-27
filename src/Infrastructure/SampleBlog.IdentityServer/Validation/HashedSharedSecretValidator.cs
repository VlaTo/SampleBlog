using IdentityModel;
using Microsoft.Extensions.Logging;
using SampleBlog.IdentityServer.Core.Extensions;
using SampleBlog.IdentityServer.Extensions;
using SampleBlog.IdentityServer.Models;
using SampleBlog.IdentityServer.Validation.Results;

namespace SampleBlog.IdentityServer.Validation;

/// <summary>
/// Validates a shared secret stored in SHA256 or SHA512
/// </summary>
public class HashedSharedSecretValidator : ISecretValidator
{
    private readonly ILogger logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="HashedSharedSecretValidator"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    public HashedSharedSecretValidator(ILogger<HashedSharedSecretValidator> logger)
    {
        this.logger = logger;
    }

    /// <summary>
    /// Validates a secret
    /// </summary>
    /// <param name="secrets">The stored secrets.</param>
    /// <param name="parsedSecret">The received secret.</param>
    /// <returns>
    /// A validation result
    /// </returns>
    /// <exception cref="System.ArgumentNullException">Id or credential</exception>
    public Task<SecretValidationResult> ValidateAsync(IEnumerable<Storage.Models.Secret> secrets, ParsedSecret parsedSecret)
    {
        var fail = Task.FromResult(new SecretValidationResult { Success = false });
        var success = Task.FromResult(new SecretValidationResult { Success = true });

        if (IdentityServerConstants.ParsedSecretTypes.SharedSecret != parsedSecret.Type)
        {
            logger.LogDebug("Hashed shared secret validator cannot process {type}", parsedSecret.Type ?? "null");
            return fail;
        }

        var sharedSecrets = secrets
            .Where(s => s.Type == IdentityServerConstants.SecretTypes.SharedSecret);

        if (false == sharedSecrets.Any())
        {
            logger.LogDebug("No shared secret configured for client.");
            return fail;
        }

        var sharedSecret = parsedSecret.Credential as string;

        if (parsedSecret.Id.IsMissing() || sharedSecret.IsMissing())
        {
            throw new ArgumentException("Id or Credential is missing.");
        }

        var secretSha256 = sharedSecret.Sha256();
        var secretSha512 = sharedSecret.Sha512();

        foreach (var secret in sharedSecrets)
        {
            var secretDescription = String.IsNullOrEmpty(secret.Description) ? "no description" : secret.Description;

            var isValid = false;
            byte[] secretBytes;

            try
            {
                secretBytes = Convert.FromBase64String(secret.Value);
            }
            catch (FormatException)
            {
                logger.LogInformation("Secret: {description} uses invalid hashing algorithm.", secretDescription);

                return fail;
            }
            catch (ArgumentNullException)
            {
                logger.LogInformation("Secret: {description} is null.", secretDescription);

                return fail;
            }

            if (32 == secretBytes.Length)
            {
                isValid = TimeConstantComparer.IsEqual(secret.Value, secretSha256);
            }
            else if (64 == secretBytes.Length)
            {
                isValid = TimeConstantComparer.IsEqual(secret.Value, secretSha512);
            }
            else
            {
                logger.LogInformation("Secret: {description} uses invalid hashing algorithm.", secretDescription);

                return fail;
            }

            if (isValid)
            {
                return success;
            }
        }

        logger.LogDebug("No matching hashed secret found.");

        return fail;
    }
}