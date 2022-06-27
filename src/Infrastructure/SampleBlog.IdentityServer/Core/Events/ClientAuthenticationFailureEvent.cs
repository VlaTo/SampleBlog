﻿using SampleBlog.IdentityServer.Models;

namespace SampleBlog.IdentityServer.Core.Events;

/// <summary>
/// Event for failed client authentication
/// </summary>
/// <seealso cref="Event" />
public class ClientAuthenticationFailureEvent : Event
{
    /// <summary>
    /// Gets or sets the client identifier.
    /// </summary>
    /// <value>
    /// The client identifier.
    /// </value>
    public string? ClientId
    {
        get;
        set;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ClientAuthenticationFailureEvent"/> class.
    /// </summary>
    /// <param name="clientId">The client identifier.</param>
    /// <param name="message">The message.</param>
    public ClientAuthenticationFailureEvent(string? clientId, string message)
        : base(EventCategories.Authentication,
            "Client Authentication Failure",
            EventTypes.Failure,
            EventIds.ClientAuthenticationFailure,
            message)
    {
        ClientId = clientId;
    }
}