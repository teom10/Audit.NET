﻿#if NETSTANDARD2_0 || NETSTANDARD1_6 || NET451
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
#else
using ActionExecutingContext = System.Web.Http.Controllers.HttpActionContext;
using ActionExecutedContext = System.Web.Http.Filters.HttpActionExecutedContext;
using System.Web.Http.Filters;
using System.Web.Http.Controllers;
using System.Net.Http;
#endif
using System;
using Audit.Core;
using System.Threading.Tasks;
using Audit.WebApi.ConfigurationApi;
using System.Threading;

namespace Audit.WebApi
{
    /// <summary>
    /// Action filter for auditing with customizable settings to be used as a global filter.
    /// This filter allows to change the settings per request, but cannot be used as an attribute to mark controllers and actions.
    /// If you don't need granular control over the settings, use the AuditApiAttribute instead.
    /// </summary>
    public class AuditApiGlobalFilter : ActionFilterAttribute
    {
        private AuditApiAdapter _adapter = new AuditApiAdapter();

        protected Func<ActionExecutingContext, bool> _logDisabledBuilder;
        protected Func<ActionExecutingContext, bool> _includeHeadersBuilder;
        protected Func<ActionExecutedContext, bool> _includeModelStateBuilder;
        protected Func<ActionExecutingContext, bool> _includeRequestBodyBuilder;
        protected Func<ActionExecutedContext, bool> _includeResponseBodyBuilder;
        protected Func<ActionExecutingContext, string> _eventTypeNameBuilder;
#if NET45
        protected Func<HttpRequestMessage, IContextWrapper> _contextWrapperBuilder;
#endif
        protected bool _serializeActionParameters;

        private AuditApiGlobalFilter()
        {
        }

        public AuditApiGlobalFilter(Action<IAuditApiGlobalActionsSelector> config)
        {
            if (config == null)
            {
                return;
            }
            var cfg = new AuditApiGlobalActionsSelector();
            config.Invoke(cfg);
            _logDisabledBuilder = cfg._config._logDisabledBuilder;
            _includeModelStateBuilder = cfg._config._includeModelStateBuilder;
            _includeHeadersBuilder = cfg._config._includeHeadersBuilder;
            _includeRequestBodyBuilder = cfg._config._includeRequestBodyBuilder;
            _includeResponseBodyBuilder = cfg._config._includeResponseBodyBuilder;
            _eventTypeNameBuilder = cfg._config._eventTypeNameBuilder;
            _serializeActionParameters = cfg._config._serializeActionParameters;
#if NET45
            _contextWrapperBuilder = cfg._config._contextWrapperBuilder;
#endif
        }

        protected bool IncludeHeaders(ActionExecutingContext actionContext)
        {
            return _includeHeadersBuilder != null ? _includeHeadersBuilder.Invoke(actionContext) : false;
        }
        protected bool IncludeModelState(ActionExecutedContext context)
        {
            return _includeModelStateBuilder != null ? _includeModelStateBuilder.Invoke(context) : false;
        }
        protected bool IncludeRequestBody(ActionExecutingContext actionContext)
        {
            return _includeRequestBodyBuilder != null ? _includeRequestBodyBuilder.Invoke(actionContext) : false;
        }
        protected bool IncludeResponseBody(ActionExecutedContext context)
        {
            return _includeResponseBodyBuilder != null ? _includeResponseBodyBuilder.Invoke(context) : false;
        }
        protected string EventTypeName(ActionExecutingContext actionContext)
        {
            return _eventTypeNameBuilder != null ? _eventTypeNameBuilder.Invoke(actionContext) : null;
        }
#if NET45
        protected IContextWrapper ContextWrapper(HttpRequestMessage request)
        {
            return _contextWrapperBuilder != null ? _contextWrapperBuilder.Invoke(request) : new ContextWrapper(request);
        }
#endif

#if NETSTANDARD2_0 || NETSTANDARD1_6 || NET451
        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (Configuration.AuditDisabled || (_logDisabledBuilder != null && _logDisabledBuilder.Invoke(context)))
            {
                await next.Invoke();
                return;
            }
            await _adapter.BeforeExecutingAsync(context, IncludeHeaders(context), IncludeRequestBody(context), _serializeActionParameters, EventTypeName(context));
            var actionExecutedContext = await next.Invoke();
            await _adapter.AfterExecutedAsync(actionExecutedContext, IncludeModelState(actionExecutedContext), IncludeResponseBody(actionExecutedContext));
        }
#else
        public override async Task OnActionExecutingAsync(ActionExecutingContext actionContext, CancellationToken cancellationToken)
        {
            if (Configuration.AuditDisabled || (_logDisabledBuilder != null && _logDisabledBuilder.Invoke(actionContext)))
            {
                return;
            }
            await _adapter.BeforeExecutingAsync(actionContext, ContextWrapper(actionContext.Request), IncludeHeaders(actionContext), 
                IncludeRequestBody(actionContext), _serializeActionParameters, EventTypeName(actionContext));
        }

        public override async Task OnActionExecutedAsync(ActionExecutedContext actionExecutedContext, CancellationToken cancellationToken)
        {
            if (Configuration.AuditDisabled)
            {
                return;
            }
            await _adapter.AfterExecutedAsync(actionExecutedContext, ContextWrapper(actionExecutedContext.Request), 
                IncludeModelState(actionExecutedContext), IncludeResponseBody(actionExecutedContext));
        }
#endif

    }
}
