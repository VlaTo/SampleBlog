﻿using Microsoft.Extensions.Logging;
using SampleBlog.IdentityServer.Models;
using SampleBlog.IdentityServer.Validation.Results;

namespace SampleBlog.IdentityServer.Validation;

/// <summary>
/// Default implementation of IScopeParser.
/// </summary>
public class DefaultScopeParser : IScopeParser
{
    private readonly ILogger<DefaultScopeParser> logger;

    /// <summary>
    /// Ctor.
    /// </summary>
    /// <param name="logger"></param>
    public DefaultScopeParser(ILogger<DefaultScopeParser> logger)
    {
        this.logger = logger;
    }

    public ParsedScopesResult ParseScopeValues(IEnumerable<string> scopeValues)
    {
        //using var activity = Tracing.ValidationActivitySource.StartActivity("DefaultScopeParser.ParseScopeValues");
        //activity?.SetTag(Tracing.Properties.Scope, scopeValues.ToSpaceSeparatedString());

        if (scopeValues == null) throw new ArgumentNullException(nameof(scopeValues));

        var result = new ParsedScopesResult();

        foreach (var scopeValue in scopeValues)
        {
            var context = new ParseScopeContext(scopeValue);

            ParseScopeValue(context);

            if (context.Succeeded)
            {
                var parsedScope = null != context.ParsedName
                    ? new ParsedScopeValue(context.RawValue, context.ParsedName, context.ParsedParameter)
                    : new ParsedScopeValue(context.RawValue);

                result.ParsedScopes.Add(parsedScope);
            }
            else if (false == context.Ignore)
            {
                result.Errors.Add(new ParsedScopeValidationError(scopeValue, context.Error));
            }
            else
            {
                logger.LogDebug("Scope parsing ignoring scope {scope}", scopeValue);
            }
        }

        return result;
    }

    /// <summary>
    /// Parses a scope value.
    /// </summary>
    /// <param name="scopeContext"></param>
    /// <returns></returns>
    public virtual void ParseScopeValue(ParseScopeContext scopeContext)
    {
        // nop leaves the raw scope value as a success result.
    }

    #region ParseScopeScontext

    /// <summary>
    /// Models the context for parsing a scope.
    /// </summary>
    public class ParseScopeContext
    {
        /// <summary>
        /// The original (raw) value of the scope.
        /// </summary>
        public string RawValue
        {
            get;
        }

        /// <summary>
        /// The parsed name of the scope. 
        /// </summary>
        public string? ParsedName
        {
            get;
            private set;
        }

        /// <summary>
        /// The parsed parameter value of the scope. 
        /// </summary>
        public string? ParsedParameter
        {
            get;
            private set;
        }

        /// <summary>
        /// The error encountered parsing the scope.
        /// </summary>
        public string? Error
        {
            get;
            private set;
        }

        /// <summary>
        /// Indicates if the scope should be excluded from the parsed results.
        /// </summary>
        public bool Ignore
        {
            get;
            private set;
        }

        /// <summary>
        /// Indicates if parsing the scope was successful.
        /// </summary>
        public bool Succeeded => !Ignore && Error == null;


        /// <summary>
        /// Ctor. Indicates success, but the scope should not be included in result.
        /// </summary>
        internal ParseScopeContext(string rawScopeValue)
        {
            RawValue = rawScopeValue;
        }

        /// <summary>
        /// Sets the parsed name and parsed parameter value for the scope.
        /// </summary>
        /// <param name="parsedName"></param>
        /// <param name="parsedParameter"></param>
        public void SetParsedValues(string parsedName, string parsedParameter)
        {
            if (String.IsNullOrWhiteSpace(parsedName))
            {
                throw new ArgumentNullException(nameof(parsedName));
            }

            if (String.IsNullOrWhiteSpace(parsedParameter))
            {
                throw new ArgumentNullException(nameof(parsedParameter));
            }

            ParsedName = parsedName;
            ParsedParameter = parsedParameter;
            Error = null;
            Ignore = false;
        }

        /// <summary>
        /// Set the error encountered parsing the scope.
        /// </summary>
        /// <param name="error"></param>
        public void SetError(string error)
        {
            ParsedName = null;
            ParsedParameter = null;
            Error = error;
            Ignore = false;
        }

        /// <summary>
        /// Sets that the scope is to be ignore/excluded from the parsed results.
        /// </summary>
        public void SetIgnore()
        {
            ParsedName = null;
            ParsedParameter = null;
            Error = null;
            Ignore = true;
        }
    }

    #endregion
}