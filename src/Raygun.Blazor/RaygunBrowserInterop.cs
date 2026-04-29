using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
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
        private readonly IJSRuntime _jsRuntime;
        private readonly RaygunSettings _raygunSettings;
        private readonly IRaygunLogger? _raygunLogger;
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

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new instance of the <see cref="RaygunBrowserInterop" /> class.
        /// </summary>
        /// <param name="jsRuntime">
        /// The <see cref="IJSRuntime" /> instance to use for JS Interop.
        /// </param>
        /// <param name="raygunSettings">
        /// The <see cref="RaygunSettings" /> instance from DI to use in configuring exception handling
        /// .</param>
        // RWM: The below attributes make sure that these methods are not removed by AOT compilation.
        //      See https://learn.microsoft.com/en-us/aspnet/core/blazor/javascript-interoperability/call-dotnet-from-javascript?view=aspnetcore-8.0#avoid-trimming-javascript-invokable-net-methods
        [DynamicDependency(nameof(RecordJsBreadcrumb))]
        [DynamicDependency(nameof(RecordJsException))]
        public RaygunBrowserInterop(IJSRuntime jsRuntime, IOptions<RaygunSettings> raygunSettings)
        {
            _dotNetReference = DotNetObjectReference.Create(this);
            _jsRuntime = jsRuntime;
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
        /// Method invoked by JavaScript to record an exception. Accepts a Raygun-owned
        /// <see cref="JsErrorPayload" /> so we never depend on browser-side WebIDL marshalling
        /// (which crashed when ErrorEvent.error was null on iOS/Safari).
        /// </summary>
        /// <param name="error">JavaScript error payload built by Raygun.Blazor.ts.</param>
        /// <param name="tags">Optional tags to attach to the report.</param>
        /// <param name="customData">Optional custom data to attach to the report.</param>
        /// <returns></returns>
        [JSInvokable]
        public async ValueTask RecordJsException(JsErrorPayload error, List<string>? tags = null, Dictionary<string, object>? customData = null)
        {
            if (error is null)
            {
                _raygunLogger?.Warning("[RaygunBrowserInterop] RecordJsException called with null payload; ignoring.");
                return;
            }

            _raygunLogger?.Verbose("[RaygunBrowserInterop] Recording JS exception: " + (error.Message ?? error.Name));

            if (_exceptionAction is null)
            {
                _raygunLogger?.Warning("[RaygunBrowserInterop] Exception action not yet wired; dropping JS exception.");
                return;
            }

            var exception = new JsUnhandledException(
                error.Name,
                error.Message,
                error.Stack,
                error.FileName,
                error.LineNumber,
                error.ColumnNumber);

            try
            {
                await _exceptionAction.Invoke(exception, null, tags, customData, CancellationToken.None);
            }
            catch (Exception ex)
            {
                // The error monitoring SDK must never become a source of unhandled errors itself.
                _raygunLogger?.Error("[RaygunBrowserInterop] Failed to record JS exception: " + ex.Message);
            }
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
        /// <param name="breadcrumbAction"></param>
        /// <param name="exceptionAction"></param>
        internal async Task InitializeAsync(
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

            // RWM: Get and cache the BrowserSpecs so we can use them later.
            BrowserSpecs = await RaygunScriptReference.InvokeAsync<BrowserSpecs>("getBrowserSpecs");
            BrowserSpecs.ParseUserAgent();
            _raygunLogger?.Verbose("[RaygunBrowserInterop] Got Browser Specs: " + BrowserSpecs);

            // RWM: Honor the developer's settings.
            if (!_raygunSettings.CatchUnhandledExceptions)
            {
                _raygunLogger?.Verbose("[RaygunBrowserInterop] Unhandled exceptions configured not being caught.");
                // RWM: Register the .NET reference so manual JS-side reportException / recordBreadcrumb still work.
                await _jsRuntime.InvokeVoidAsync("window.raygunBlazor.initialize", _dotNetReference);
                return;
            }

            // RWM: Register the .NET reference with JS. The JS side wires window 'error' and
            //      'unhandledrejection' listeners that call back into RecordJsException.
            await _jsRuntime.InvokeVoidAsync("window.raygunBlazor.initialize", _dotNetReference);
            _raygunLogger?.Verbose("[RaygunBrowserInterop] Registered .NET reference and unhandled JS exception handlers.");
        }

        #endregion

        #region Interface Implementations

        /// <summary>
        /// Properly dispose of all of the JS resources used so we don't cause memory leaks.
        /// </summary>
        public async ValueTask DisposeAsync()
        {
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
