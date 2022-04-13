﻿using Microsoft.IdentityModel.Tokens;
using SampleBlog.IdentityServer.Models;

namespace SampleBlog.IdentityServer.Services;

/// <summary>
/// Interface for the key material service
/// </summary>
public interface IKeyMaterialService
{
    /// <summary>
    /// Gets all validation keys.
    /// </summary>
    /// <returns></returns>
    Task<IEnumerable<SecurityKeyInfo>> GetValidationKeysAsync();

    /// <summary>
    /// Gets the signing credentials.
    /// </summary>
    /// <param name="allowedAlgorithms">Collection of algorithms used to filter the server supported algorithms. 
    /// A value of null or empty indicates that the server default should be returned.</param>
    /// <returns></returns>
    Task<SigningCredentials> GetSigningCredentialsAsync(IEnumerable<string>? allowedAlgorithms = null);

    /// <summary>
    /// Gets all signing credentials.
    /// </summary>
    /// <returns></returns>
    Task<IEnumerable<SigningCredentials>> GetAllSigningCredentialsAsync();
}