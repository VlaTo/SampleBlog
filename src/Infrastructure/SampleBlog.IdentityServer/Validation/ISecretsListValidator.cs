﻿using SampleBlog.IdentityServer.Models;
using SampleBlog.IdentityServer.Validation.Results;
using Secret = SampleBlog.IdentityServer.Storage.Models.Secret;

namespace SampleBlog.IdentityServer.Validation;

/// <summary>
/// Validator for an Enumerable List of Secrets
/// </summary>
public interface ISecretsListValidator
{
    /// <summary>
    /// Validates a list of secrets
    /// </summary>
    /// <param name="secrets">The stored secrets.</param>
    /// <param name="parsedSecret">The received secret.</param>
    /// <returns>A validation result</returns>
    Task<SecretValidationResult> ValidateAsync(IEnumerable<Secret> secrets, ParsedSecret parsedSecret);
}