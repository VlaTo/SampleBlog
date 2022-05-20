using IdentityModel;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SampleBlog.IdentityServer.Core;
using SampleBlog.IdentityServer.Core.Events;
using SampleBlog.IdentityServer.DependencyInjection.Options;
using SampleBlog.IdentityServer.Endpoints.Results;
using SampleBlog.IdentityServer.Extensions;
using SampleBlog.IdentityServer.Hosting;
using SampleBlog.IdentityServer.Models;
using SampleBlog.IdentityServer.ResponseHandling;
using SampleBlog.IdentityServer.Services;
using SampleBlog.IdentityServer.Stores;
using SampleBlog.IdentityServer.Validation.Requests;
using System.Collections.Specialized;
using System.Security.Claims;

namespace SampleBlog.IdentityServer.Endpoints;

internal abstract class AuthorizeEndpointBase : IEndpointHandler
{
    private readonly IAuthorizeResponseGenerator authorizeResponseGenerator;
    private readonly IEventService events;
    private readonly IdentityServerOptions options;
    private readonly IAuthorizeInteractionResponseGenerator interactionGenerator;
    private readonly IAuthorizeRequestValidator validator;
    private readonly IConsentMessageStore consentResponseStore;
    private readonly IAuthorizationParametersMessageStore? authorizationParametersMessageStore;

    protected ILogger Logger
    {
        get;
    }

    protected IUserSession UserSession
    {
        get;
    }

    protected AuthorizeEndpointBase(
        IEventService events,
        ILogger<AuthorizeEndpointBase> logger,
        IdentityServerOptions options,
        IAuthorizeRequestValidator validator,
        IAuthorizeInteractionResponseGenerator interactionGenerator,
        IAuthorizeResponseGenerator authorizeResponseGenerator,
        IUserSession userSession,
        IConsentMessageStore consentResponseStore,
        IAuthorizationParametersMessageStore? authorizationParametersMessageStore = null)
    {
        this.events = events;
        this.options = options;
        this.validator = validator;
        this.interactionGenerator = interactionGenerator;
        this.authorizeResponseGenerator = authorizeResponseGenerator;
        this.consentResponseStore = consentResponseStore;
        this.authorizationParametersMessageStore = authorizationParametersMessageStore;
        
        Logger = logger;
        UserSession = userSession;
    }

    public abstract Task<IEndpointResult?> ProcessAsync(HttpContext context);

    internal async Task<IEndpointResult?> ProcessAuthorizeRequestAsync(
        NameValueCollection parameters,
        ClaimsPrincipal? user,
        bool checkConsentResponse = false)
    {
        if (null != user)
        {
            Logger.LogDebug("User in authorize request: {subjectId}", user.GetSubjectId());
        }
        else
        {
            Logger.LogDebug("No user present in authorize request");
        }

        if (checkConsentResponse && null != authorizationParametersMessageStore)
        {
            var messageStoreId = parameters[Constants.AuthorizationParamsStore.MessageStoreIdParameterName];
            var entry = await authorizationParametersMessageStore.ReadAsync(messageStoreId);
            
            parameters = entry?.Data.FromFullDictionary() ?? new NameValueCollection();

            await authorizationParametersMessageStore.DeleteAsync(messageStoreId);
        }

        // validate request
        var result = await validator.ValidateAsync(parameters, user);

        if (result.IsError)
        {
            return await CreateErrorResultAsync(
                "Request validation failed",
                result.ValidatedRequest,
                result.Error,
                result.ErrorDescription);
        }

        string? consentRequestId = null;

        try
        {
            Message<ConsentResponse>? consent = null;

            if (checkConsentResponse)
            {
                var consentRequest = new ConsentRequest(result.ValidatedRequest.Raw, user?.GetSubjectId());
                
                consentRequestId = consentRequest.Id;

                consent = await consentResponseStore.ReadAsync(consentRequestId);

                if (consent is { Data: null })
                {
                    return await CreateErrorResultAsync("consent message is missing data");
                }
            }

            var request = result.ValidatedRequest;
            //LogRequest(request);

            // determine user interaction
            var interactionResult = await interactionGenerator.ProcessInteractionAsync(request, consent?.Data);

            if (interactionResult.IsError)
            {
                return await CreateErrorResultAsync("Interaction generator error", request, interactionResult.Error, interactionResult.ErrorDescription, false);
            }

            if (interactionResult.IsLogin)
            {
                return new LoginPageResult(request);
            }

            if (interactionResult.IsConsent)
            {
                return new ConsentPageResult(request);
            }

            if (interactionResult.IsRedirect)
            {
                return new CustomRedirectResult(request, interactionResult.RedirectUrl);
            }

            var response = await authorizeResponseGenerator.CreateResponseAsync(request);

            await RaiseResponseEventAsync(response);

            //LogResponse(response);

            return new AuthorizeResult(response);
        }
        finally
        {
            if (null != consentRequestId)
            {
                await consentResponseStore.DeleteAsync(consentRequestId);
            }
        }
    }

    protected async Task<IEndpointResult> CreateErrorResultAsync(
        string logMessage,
        ValidatedAuthorizeRequest? request = null,
        string? error = OidcConstants.AuthorizeErrors.ServerError,
        string? errorDescription = null,
        bool logError = true)
    {
        if (logError)
        {
            Logger.LogError(logMessage);
        }

        if (null != request)
        {
            //var details = new AuthorizeRequestValidationLog(request, options.Logging.AuthorizeRequestSensitiveValuesFilter);
            //Logger.LogInformation("{@validationDetails}", details);
        }

        // TODO: should we raise a token failure event for all errors to the authorize endpoint?
        await RaiseFailureEventAsync(request, error, errorDescription);

        return new AuthorizeResult(new AuthorizeResponse
        {
            Request = request,
            Error = error,
            ErrorDescription = errorDescription,
            SessionState = request?.GenerateSessionStateValue()
        });
    }
    
    private Task RaiseFailureEventAsync(
        ValidatedAuthorizeRequest? request,
        string error,
        string? errorDescription)
    {
        return events.RaiseAsync(new TokenIssuedFailureEvent(request, error, errorDescription));
    }

    private Task RaiseResponseEventAsync(AuthorizeResponse response)
    {
        if (false == response.IsError)
        {
            //LogTokens(response);
            return events.RaiseAsync(new TokenIssuedSuccessEvent(response));
        }

        return RaiseFailureEventAsync(response.Request, response.Error, response.ErrorDescription);
    }
}