﻿using SampleBlog.IdentityServer.Contexts;
using SampleBlog.IdentityServer.Validation.Contexts;

namespace SampleBlog.IdentityServer.Services;

/// <summary>
/// This interface allows IdentityServer to connect to your user and profile store.
/// </summary>
public interface IProfileService
{
    /// <summary>
    /// This method is called whenever claims about the user are requested (e.g. during token creation or via the userinfo endpoint)
    /// </summary>
    /// <param name="context">The context.</param>
    /// <returns></returns>
    Task GetProfileDataAsync(ProfileDataRequestContext context);

    /// <summary>
    /// This method gets called whenever identity server needs to determine if the user is valid or active (e.g. if the user's account has been deactivated since they logged in).
    /// (e.g. during token issuance or validation).
    /// </summary>
    /// <param name="context">The context.</param>
    /// <returns></returns>
    Task IsActiveAsync(IsActiveContext context);
}