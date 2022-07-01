﻿using IdentityModel;
using System.Security.Claims;

namespace SampleBlog.IdentityServer.Validation.Results;

/// <summary>
/// Models the result of custom grant validation.
/// </summary>
public class GrantValidationResult : ValidationResult
{
    /// <summary>
    /// Gets or sets the principal which represents the result of the validation.
    /// </summary>
    /// <value>
    /// The principal.
    /// </value>
    public ClaimsPrincipal? Subject
    {
        get;
        set;
    }

    /// <summary>
    /// Custom fields for the token response
    /// </summary>
    public Dictionary<string, object> CustomResponse
    {
        get;
        set;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GrantValidationResult"/> class with no subject.
    /// Warning: the resulting access token will only contain the client identity.
    /// </summary>
    public GrantValidationResult()
    {
        IsError = false;
        CustomResponse = new Dictionary<string, object>();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GrantValidationResult"/> class with no subject.
    /// Warning: the resulting access token will only contain the client identity.
    /// </summary>
    public GrantValidationResult(Dictionary<string, object>? customResponse = null)
        : this()
    {
        if (null != customResponse)
        {
            foreach (var kvp in customResponse)
            {
                CustomResponse[kvp.Key] = kvp.Value;
            }
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GrantValidationResult"/> class with a given principal.
    /// Warning: the principal needs to include the required claims - it is recommended to use the other constructor that does validation.
    /// </summary>
    public GrantValidationResult(
        ClaimsPrincipal principal,
        Dictionary<string, object>? customResponse = null)
        : this(customResponse)
    {
        if (1 != principal.Identities.Count())
        {
            throw new InvalidOperationException("only a single identity supported");
        }

        if (null == principal.FindFirst(JwtClaimTypes.Subject))
        {
            throw new InvalidOperationException("sub claim is missing");
        }

        if (null == principal.FindFirst(JwtClaimTypes.IdentityProvider))
        {
            throw new InvalidOperationException("idp claim is missing");
        }

        if (null == principal.FindFirst(JwtClaimTypes.AuthenticationMethod))
        {
            throw new InvalidOperationException("amr claim is missing");
        }

        if (null == principal.FindFirst(JwtClaimTypes.AuthenticationTime))
        {
            throw new InvalidOperationException("auth_time claim is missing");
        }

        Subject = principal;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GrantValidationResult"/> class with an error and description.
    /// </summary>
    /// <param name="error">The error.</param>
    /// <param name="errorDescription">The error description.</param>
    /// <param name="customResponse">Custom response elements</param>
    public GrantValidationResult(
        TokenRequestErrors error,
        string? errorDescription = null,
        Dictionary<string, object>? customResponse = null)
        : this(customResponse)
    {
        Error = ConvertTokenErrorEnumToString(error);
        ErrorDescription = errorDescription;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GrantValidationResult" /> class.
    /// </summary>
    /// <param name="subject">The subject claim used to uniquely identifier the user.</param>
    /// <param name="authenticationMethod">The authentication method which describes the custom grant type.</param>
    /// <param name="claims">Additional claims that will be maintained in the principal. 
    /// If you want these claims to appear in token, you need to add them explicitly in your custom implementation of <see cref="Services.IProfileService"/> service.</param>
    /// <param name="identityProvider">The identity provider.</param>
    /// <param name="customResponse">The custom response.</param>
    public GrantValidationResult(
        string subject,
        string authenticationMethod,
        IEnumerable<Claim>? claims = null,
        string? identityProvider = IdentityServerConstants.LocalIdentityProvider,
        Dictionary<string, object>? customResponse = null)
        : this(subject, authenticationMethod, DateTime.UtcNow, claims, identityProvider, customResponse)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GrantValidationResult" /> class.
    /// </summary>
    /// <param name="subject">The subject claim used to uniquely identifier the user.</param>
    /// <param name="authenticationMethod">The authentication method which describes the custom grant type.</param>
    /// <param name="authTime">The UTC date/time of authentication.</param>
    /// <param name="claims">Additional claims that will be maintained in the principal.
    /// If you want these claims to appear in token, you need to add them explicitly in your custom implementation of <see cref="Services.IProfileService"/> service.</param>
    /// <param name="identityProvider">The identity provider.</param>
    /// <param name="customResponse">The custom response.</param>
    public GrantValidationResult(
        string subject,
        string authenticationMethod,
        DateTime authTime,
        IEnumerable<Claim>? claims = null,
        string? identityProvider = IdentityServerConstants.LocalIdentityProvider,
        Dictionary<string, object>? customResponse = null)
        : this(customResponse)
    {
        var resultClaims = new List<Claim>
        {
            new(JwtClaimTypes.Subject, subject),
            new(JwtClaimTypes.AuthenticationMethod, authenticationMethod),
            new(JwtClaimTypes.IdentityProvider, identityProvider),
            new(JwtClaimTypes.AuthenticationTime, new DateTimeOffset(authTime).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
        };

        if (null == claims)
        {
            resultClaims.AddRange(claims);
        }

        var id = new ClaimsIdentity(authenticationMethod);

        id.AddClaims(resultClaims.Distinct(new ClaimComparer()));

        Subject = new ClaimsPrincipal(id);
    }

    private static string ConvertTokenErrorEnumToString(TokenRequestErrors error)
    {
        return error switch
        {
            TokenRequestErrors.InvalidClient => OidcConstants.TokenErrors.InvalidClient,
            TokenRequestErrors.InvalidGrant => OidcConstants.TokenErrors.InvalidGrant,
            TokenRequestErrors.InvalidRequest => OidcConstants.TokenErrors.InvalidRequest,
            TokenRequestErrors.InvalidScope => OidcConstants.TokenErrors.InvalidScope,
            TokenRequestErrors.UnauthorizedClient => OidcConstants.TokenErrors.UnauthorizedClient,
            TokenRequestErrors.UnsupportedGrantType => OidcConstants.TokenErrors.UnsupportedGrantType,
            TokenRequestErrors.InvalidTarget => OidcConstants.TokenErrors.InvalidTarget,
            _ => throw new InvalidOperationException("invalid token error")
        };
    }
}