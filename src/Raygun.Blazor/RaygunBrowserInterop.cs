﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using KristofferStrube.Blazor.DOM;
using KristofferStrube.Blazor.WebIDL;
using KristofferStrube.Blazor.WebIDL.Exceptions;
using KristofferStrube.Blazor.Window;
using Microsoft.Extensions.Options;
using Microsoft.JSInterop;
using Raygun.Blazor.Logging;
using Raygun.Blazor.Models;

namespace Raygun.Blazor
{
    /// <summary>
    /// Handles interoperability between the <see cref="RaygunBlazorClient" /> and JavaScript.
    /// </summary>
    public class RaygunBrowserInterop : IAsyncDisposable
    {
        #region Private Members

        private readonly DotNetObjectReference<RaygunBrowserInterop> _dotNetReference;
        private EventListener<ErrorEvent>? _errorEventListener;
        private readonly IJSRuntime _jsRuntime;
        private readonly RaygunSettings _raygunSettings;
        private readonly IRaygunLogger? _raygunLogger;
        private readonly IWindowService _windowService;
        private Action<string, BreadcrumbType, string?, Dictionary<string, object>?, string?, BreadcrumbLevel>? _breadcrumbAction;
        private Func<Exception, UserDetails?, List<string>?, Dictionary<string, object>?, CancellationToken, Task>? _exceptionAction;

        #endregion

        #region Public Properties

        /// <summary>
        /// A dynamic reference to the Raygun Blazor Script, so developers don't have to manually add a script file
        /// to their index.html page.
        /// </summary>
        public IJSObjectReference? RaygunScriptReference { get; private set; }

        #endregion

        #region Internal Properties

        /// <summary>
        /// Details about the Browser that are highly unlikely to change at runtime.
        /// </summary>
        internal BrowserSpecs? BrowserSpecs { get; private set; }

        /// <summary>
        /// Details about the Browser that are retrieved on every error report.
        /// </summary>
        internal BrowserStats? LatestBrowserStats { get; private set; }

        /// <summary>
        /// A reference to the Browser's Window object, in case we want to do other things with it.
        /// </summary>
        internal Window? Window { get; private set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new instance of the <see cref="RaygunBrowserInterop" /> class.
        /// </summary>
        /// <param name="jsRuntime">
        /// The <see cref="IJSRuntime" /> instance to use for JS Interop.
        /// </param>
        /// <param name="windowService">
        /// The <see cref="WindowService" /> used to get a reference to the Browser Window object.
        /// </param>
        /// <param name="raygunSettings">
        /// The <see cref="RaygunSettings" /> instance from DI to use in configuring exception handling
        /// .</param>
        // RWM: The below attributes make sure that these methods are not removed by AOT compilation.
        //      See https://learn.microsoft.com/en-us/aspnet/core/blazor/javascript-interoperability/call-dotnet-from-javascript?view=aspnetcore-8.0#avoid-trimming-javascript-invokable-net-methods
        [DynamicDependency(nameof(RecordJsBreadcrumb))]
        [DynamicDependency(nameof(RecordJsException))]
        public RaygunBrowserInterop(IJSRuntime jsRuntime, IWindowService windowService,
            IOptions<RaygunSettings> raygunSettings)
        {
            _dotNetReference = DotNetObjectReference.Create(this);
            _jsRuntime = jsRuntime;
            _windowService = windowService;
            _raygunSettings = raygunSettings.Value;
            _raygunLogger = RaygunLogger.Create(raygunSettings.Value.LogLevel);
            _raygunLogger?.Verbose("[RaygunBrowserInterop] Created.");
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Method invoked by JavaScript to record a breadcrumb.
        /// See Raygun.Blazor.ts for more information.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="breadcrumbType"></param>
        /// <param name="category"></param>
        /// <param name="level"></param>
        /// <param name="customData"></param>
        /// <returns></returns>
        [JSInvokable]
        public ValueTask RecordJsBreadcrumb(string message, BreadcrumbType breadcrumbType = BreadcrumbType.Manual,
            string? category = null, BreadcrumbLevel level = Models.BreadcrumbLevel.Info, Dictionary<string, object>? customData = null)
        {
            _raygunLogger?.Verbose("[RaygunBrowserInterop] Recording breadcrumb: " + message);
            _breadcrumbAction!.Invoke(message, breadcrumbType, category, customData, "JavaScript", level);
            return ValueTask.CompletedTask;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="error">JavaScript Error object</param>
        /// <param name="tags"></param>
        /// <param name="customData"></param>
        /// <returns></returns>
        [JSInvokable]
        public async ValueTask RecordJsException(JSError error, List<string>? tags = null, Dictionary<string, object>? customData = null)
        {
            _raygunLogger?.Verbose("[RaygunBrowserInterop] Recording JS exception: " + error.Message);
            var exception = new WebIDLException($"{error.Name}: \"{error.Message}\"", error.Stack, error.InnerException);
            await _exceptionAction!.Invoke(exception, null, tags, customData, CancellationToken.None);
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Sets the <see cref="LatestBrowserStats" /> to a fresh instance, makes it available to other code if necessary,
        /// and returns a new instance of <see cref="EnvironmentDetails" /> populated with both the <see cref="BrowserSpecs" />
        /// and the <see cref="LatestBrowserStats" />.
        /// </summary>
        /// <returns>
        /// Returns a new instance of <see cref="EnvironmentDetails" /> populated with both the <see cref="BrowserSpecs" />
        /// and the <see cref="LatestBrowserStats" />.
        /// </returns>
        internal async Task<EnvironmentDetails> GetBrowserEnvironment()
        {
            LatestBrowserStats ??= await RaygunScriptReference!.InvokeAsync<BrowserStats>("getBrowserStats");
            _raygunLogger?.Verbose("[RaygunBrowserInterop] getBrowserEnvironment: " + LatestBrowserStats);
            // RWM: Combine the Specs we got on Initialize() with the stats we just grabbed to build the Environment.
            return new EnvironmentDetails(BrowserSpecs, LatestBrowserStats);
        }

        /// <summary>
        /// Properly configures Raygun Blazor for use with JavaScript.
        /// </summary>
        /// <param name="onUnhandledJsException"></param>
        /// <param name="breadcrumbAction"></param>
        /// <param name="exceptionAction"></param>
        internal async Task InitializeAsync(Func<ErrorEvent, Task> onUnhandledJsException,
            Action<string, BreadcrumbType, string?, Dictionary<string, object>?, string?, BreadcrumbLevel>? breadcrumbAction,
            Func<Exception, UserDetails?, List<string>?, Dictionary<string, object>?, CancellationToken, Task>? exceptionAction)
        {
            _breadcrumbAction = breadcrumbAction;
            _exceptionAction = exceptionAction;

            // RWM: We're going to register the Raygun script and get the BrowserSpecs first. The reason why is because if we
            //      handle JS errors & they start coming in before we're ready, then there will be wailing and gnashing of teeth.
            RaygunScriptReference = await _jsRuntime.InvokeAsync<IJSObjectReference>("import",
                "./_content/Raygun.Blazor/Raygun.Blazor.js");
            _raygunLogger?.Verbose("[RaygunBrowserInterop] Registered Raygun Blazor script.");

            // RWM: Register the .NET reference with JS so that JS code can also manually create Bookmarks and report Exceptions.
            await _jsRuntime.InvokeVoidAsync("window.raygunBlazor.initialize", _dotNetReference);
            _raygunLogger?.Verbose("[RaygunBrowserInterop] Registered .NET reference with JS.");

            // RWM: Get and cache the BrowserSpecs so we can use them later.
            BrowserSpecs = await RaygunScriptReference.InvokeAsync<BrowserSpecs>("getBrowserSpecs");
            BrowserSpecs.ParseUserAgent();
            _raygunLogger?.Verbose("[RaygunBrowserInterop] Got Browser Specs: " + BrowserSpecs);

            // RWM: Honor the developer's settings.
            if (!_raygunSettings.CatchUnhandledExceptions)
            {
                _raygunLogger?.Verbose("[RaygunBrowserInterop] Unhandled exceptions configured not being caught.");
                return;
            }

            // RWM: Register a handler for unhandled JS exceptions.
            Window = await _windowService.GetWindowAsync();
            _errorEventListener = await EventListener<ErrorEvent>.CreateAsync(_jsRuntime, onUnhandledJsException);
            await Window.AddOnErrorEventListenerAsync(_errorEventListener);
            _raygunLogger?.Verbose("[RaygunBrowserInterop] Registered unhandled JS exception handler.");
        }

        #endregion

        #region Interface Implementations

        /// <summary>
        /// Properly dispose of all of the JS resources used so we don't cause memory leaks.
        /// </summary>
        public async ValueTask DisposeAsync()
        {
            if (_errorEventListener is not null && Window is not null)
            {
                await Window.RemoveOnErrorEventListenerAsync(_errorEventListener);
                await _errorEventListener.DisposeAsync();
            }

            if (Window is not null)
            {
                await Window.DisposeAsync();
            }

            if (RaygunScriptReference is not null)
            {
                await RaygunScriptReference.DisposeAsync();
            }

            if (_dotNetReference is not null)
            {
                _dotNetReference.Dispose();
            }

            _raygunLogger?.Verbose("[RaygunBrowserInterop] Disposed.");
        }

        #endregion
    }
}