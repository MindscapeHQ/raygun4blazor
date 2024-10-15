using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using KristofferStrube.Blazor.WebIDL.Exceptions;
using KristofferStrube.Blazor.Window;
using Microsoft.Extensions.Options;
using Raygun.Blazor.Events;
using Raygun.Blazor.Interfaces;
using Raygun.Blazor.Logging;
using Raygun.Blazor.Models;
using Raygun.Blazor.Offline;
using Raygun.Blazor.Queue;

namespace Raygun.Blazor
{
    /// <summary>
    /// A Raygun client designed specifically for Blazor applications.
    /// </summary>
    public class RaygunBlazorClient
    {
        #region Public Members

        /// <summary>
        /// Raised just before a message is sent. This can be used to make final adjustments to the <see cref="RaygunRequest"/>, or to cancel the send.
        /// </summary>
        public event EventHandler<RaygunRequestSendEventArgs>? OnBeforeSend;

        #endregion

        #region Private Members

        // private readonly IRaygunOfflineStore? _offlineStore;
        // private readonly IRaygunQueueManager? _queueManager;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IRaygunLogger? _raygunLogger;
        private readonly IRaygunUserProvider? _userManager;
        private readonly JsonSerializerOptions _jsonOptions;
        private readonly List<BreadcrumbDetails> _breadcrumbs;
        private readonly RaygunBrowserInterop _browserInterop;
        private readonly RaygunSettings _raygunSettings;
        private readonly ThrottledBackgroundMessageProcessor? _messageProcessor;
        private readonly IRaygunOfflineStore? _offlineStore;

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new instance of the <see cref="RaygunBlazorClient" /> class.
        /// </summary>
        /// <param name="raygunSettings">The <see cref="RaygunSettings" /> injected into the DI container by calling services.AddRaygunBlazor().</param>
        /// <param name="httpClientFactory">The <see cref="IHttpClientFactory" /> injected into the DI container by calling services.AddRaygunBlazor().</param>
        /// <param name="browserInterop"></param>
        /// <param name="userManager">Optional, provides <see cref="UserDetails"/> to attach to reported errors.</param>
        /// <param name="offlineStore">Optional, stores Raygun requests that failed to send to try again later.</param>
        /// <remarks>
        /// You should not usually create a new instance yourself, instead get a usable instance from the DI container by injecting it into the Blazor page directly.
        /// </remarks>
        public RaygunBlazorClient(IOptions<RaygunSettings> raygunSettings, IHttpClientFactory httpClientFactory,
            RaygunBrowserInterop browserInterop, IRaygunUserProvider? userManager = null, IRaygunOfflineStore? offlineStore = null)
        {
            RaygunSettings settings = raygunSettings.Value;
            _raygunLogger = RaygunLogger.Create(settings.LogLevel);
            _userManager = userManager;

            // RWM: We do this first because there is no point consuming CPU cycles setting properties if the API key is missing.
            _raygunSettings = settings;
            if (string.IsNullOrWhiteSpace(_raygunSettings.ApiKey))
            {
                _raygunLogger?.Error(
                    "[RaygunBlazorClient] A Raygun API Key was not provided. Please check your settings and try again.");
                // ReSharper disable once NotResolvedInText
                throw new ArgumentNullException("RaygunSettings.ApiKey",
                    "A Raygun API Key was not provided. Please check your settings and try again.");
            }

            _breadcrumbs = [];
            _browserInterop = browserInterop;
            _httpClientFactory = httpClientFactory;
            _jsonOptions = new JsonSerializerOptions
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            };

            if (settings.UseBackgroundQueue)
            {
                _messageProcessor = new ThrottledBackgroundMessageProcessor(settings.BackgroundMessageQueueMax,
                    settings.BackgroundMessageWorkerCount, settings.BackgroundMessageWorkerBreakpoint, Send,
                    _raygunLogger);
            }

            // Setup offline store and callback
            _offlineStore = offlineStore;
            _offlineStore?.SetSendCallback(SendOfflinePayloadAsync);

            _raygunLogger?.Debug("[RaygunBlazorClient] Initialized.");
        }

        #endregion


        #region Public Methods

        /// <summary>
        /// Initializes the JavaScript interoperability for this <see cref="RaygunBlazorClient"/>.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// This method should be called as early as possible in the course of using the app. The best place to do it is inside the
        /// OnAfterRenderAsync method of the main layout page. However it will also be called automatically before sending any
        /// exceptions, just in case.
        /// </remarks>
        /// <code>
        /// @inherits LayoutComponentBase
        /// @inject RaygunBlazorClient raygunClient;
        /// 
        /// <div class="page">
        ///     <div class="sidebar">
        ///         <NavMenu />
        ///     </div>
        /// 
        ///     <main>
        ///         <div class="top-row px-4">
        ///             <a href="https://learn.microsoft.com/aspnet/core/" target="_blank">About</a>
        ///         </div>
        /// 
        ///         <article class="content px-4">
        ///             @Body
        ///         </article>
        ///     </main>
        /// </div>
        /// 
        /// @code {
        ///     protected override async Task OnAfterRenderAsync(bool firstRender)
        ///     {
        ///         if (firstRender)
        ///         {
        ///             await raygunClient.InitializeAsync();
        ///         }
        ///         await base.OnAfterRenderAsync(firstRender);
        ///     }
        /// }
        /// </code>
        public async Task InitializeAsync()
        {
            if (_browserInterop.RaygunScriptReference is null)
            {
                await _browserInterop.InitializeAsync(OnUnhandledJsException, RecordBreadcrumb, RecordExceptionAsync);
                _raygunLogger?.Debug("[RaygunBlazorClient] JavaScript Interop initialized.");
            }
        }

        /// <summary>
        /// Records a Breadcrumb to help you track what was going on in your application before an error occurred.
        /// </summary>
        /// <param name="message">The message you want to record for this Breadcrumb.</param>
        /// <param name="type">The <see cref="BreadcrumbType"> for the message. Defaults to <see cref="BreadcrumbType.Manual"/>.</param>
        /// <param name="category">A custom value used to arbitrarily group this Breadcrumb.</param>
        /// <param name="customData">Any custom data you want to record about application state when the Breadcrumb was recorded.</param>
        /// <remarks>
        /// Breadcrumbs will be queued internally by the <see cref="RaygunBlazorClient" /> and sent with the next Exception report.
        /// </remarks>
        /// <code>
        /// TBD
        /// </code>
        public void RecordBreadcrumb(string? message, BreadcrumbType breadcrumbType = BreadcrumbType.Manual,
            string? category = null, Dictionary<string, object>? customData = null, string? platform = "DotNet")
        {
            _breadcrumbs.Add(new BreadcrumbDetails(message, breadcrumbType, category, customData, platform));
            _raygunLogger?.Verbose("[RaygunBlazorClient] Breadcrumb recorded: " + message);
        }


        /// <summary>
        /// Queues an exception to be sent to Raygun.
        /// </summary>
        /// <param name="ex">The exception to be sent back to Raygun.</param>
        /// <param name="userDetails">Attach user details to exception, takes priority over <see cref="IRaygunUserProvider" /></param>
        /// <param name="userCustomData">Any custom data that you you like sent with the report to assist with troubleshooting.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken" /> to allow you to cancel the current request, if necessary.</param>
        /// <param name="tags">User-specified tags that should be applied to the error.</param>
        /// <returns></returns>
        /// <code>
        /// TBD
        /// </code>
        public async Task RecordExceptionAsync(Exception ex,
            UserDetails? userDetails = null,
            List<string>? tags = null,
            Dictionary<string, object>? userCustomData = null,
            CancellationToken cancellationToken = default)
        {
            _raygunLogger?.Verbose("[RaygunBlazorClient] Recording exception: " + ex);
            await InitializeAsync();

            var appVersion = _raygunSettings.ApplicationVersion ??
                             System.Reflection.Assembly.GetEntryAssembly()?.GetName().Version?.ToString() ??
                             null;

            UserDetails? user = null;
            // User details provided in method call
            if (userDetails != null)
            {
                user = userDetails;
            }
            // If the user details are not provided, attempt to get them from the UserManager.
            else if (_userManager != null)
            {
                user = await _userManager!.GetCurrentUser();
            }

            EnvironmentDetails? environment;
            if (OperatingSystem.IsBrowser() || OperatingSystem.IsIOS() || OperatingSystem.IsAndroid())
            {
                // If running on browser (e.g. WebAssembly)
                // or Mobile MAUI Blazor Hybrid apps (iOS or Android),
                // obtain environment details from the browser
                environment = await _browserInterop.GetBrowserEnvironment();
            }
            else
            {
                // If running on Server (Linux, Windows, MacOS, etc.)
                // or Desktop MAUI Hybrid apps (Windows or Mac Catalyst),
                // obtain environment details from the runtime
                environment = EnvironmentDetails.FromRuntime();
                // Add user browser details to userCustomData
                userCustomData ??= [];
                userCustomData.Add("BrowserEnvironment", await _browserInterop.GetBrowserEnvironment());
            }


            var request = new RaygunRequest
            {
                Details = new EventDetails(appVersion)
                {
                    Breadcrumbs = _breadcrumbs.ToList(),
                    Environment = environment,
                    Error = new ErrorDetails(ex),
                    Tags = tags,
                    User = user,
                    UserCustomData = userCustomData,
                }
            };

            // Allow user to modify or cancel the send
            if (OnBeforeSend != null)
            {
                var args = new RaygunRequestSendEventArgs(request);
                OnBeforeSend(this, args);
                if (args.Cancel)
                {
                    _raygunLogger?.Debug("[RaygunBlazorClient] Request send cancelled by event handler.");
                    return;
                }
            }

            _raygunLogger?.Debug("[RaygunBlazorClient] Sending request to Raygun: " + request);

            if (_messageProcessor != null)
            {
                // If we're using a background queue, enqueue the request.
                if (_messageProcessor.Enqueue(request))
                {
                    _raygunLogger?.Debug("[RaygunBlazorClient] Request enqueued for background processing.");
                }
                else
                {
                    _raygunLogger?.Error("[RaygunBlazorClient] Failed to enqueue request for background processing.");
                }
            }
            else
            {
                // Otherwise, send the request immediately.
                await Send(request, cancellationToken);
            }
        }


        /// <summary>
        /// Allows you to immediately send any queued data to Raygun.
        /// </summary>
        /// <remarks>This is useful if you want to ensure the data is sent before the user navigates away from the app.</remarks>
        /// <returns></returns>
        /// <remarks>Will be implemented in the next update.</remarks>
        public async Task FlushAsync()
        {
            await Task.CompletedTask;
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Called by the Offline Store to send a stored request.
        /// </summary>
        /// <param name="request">The stored <see cref="RaygunRequest" /> to send.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken" /> to allow you to cancel the current request, if necessary.</param>
        /// <returns>True if the request is sent successfully</returns>
        /// <remarks>
        /// If the request is sent successfully, the offline store will delete the stored request.
        /// Otherwise, the stored request will remain and tried again later.
        /// </remarks>
        private async Task<bool> SendOfflinePayloadAsync(RaygunRequest request, CancellationToken cancellationToken)
        {
            // Send with offline store disabled to prevent storing the same message again.
            return await Send(request, cancellationToken, useOfflineStore: false).ConfigureAwait(false);
        }

        /// <summary>
        /// Sends a <see cref="RaygunRequest" /> to the Raygun API.
        /// This method overloads the Send method, hiding the useOfflineStore parameter and setting it to true for convenience.
        /// </summary>
        /// <param name="request">The <see cref="RaygunRequest" /> to send.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken" /> to allow you to cancel the current request, if necessary.</param>
        /// <returns>True if the request is sent successfully</returns>
        private async Task<bool> Send(RaygunRequest request, CancellationToken cancellationToken)
        {
            return await Send(request, cancellationToken, useOfflineStore: true);
        }

        /// <summary>
        /// Sends a <see cref="RaygunRequest" /> to the Raygun API.
        /// </summary>
        /// <param name="request">The <see cref="RaygunRequest" /> to send.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken" /> to allow you to cancel the current request, if necessary.</param>
        /// <param name="useOfflineStore">Whether to use the offline store to save the request if it fails to send.</param>
        /// <returns>True if the request is sent successfully</returns>
        private async Task<bool> Send(RaygunRequest request, CancellationToken cancellationToken, bool useOfflineStore)
        {
            bool shouldStoreMessage = false;

            try
            {
                // RWM: This is temporary for now to ensure the request is properly created.
                var client = _httpClientFactory.CreateClient("Raygun");
                var response = await client.PostAsJsonAsync(RaygunSettings.EntriesEndpoint, request, _jsonOptions,
                    cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    _raygunLogger?.Error(
                        "[RaygunBlazorClient] Failed to send request to Raygun: " + response.StatusCode);

                    // Only store messages that failed to send due to server error (e.g. error 500+)
                    // Errors like 400 Bad Request are likely due to invalid data and should not be stored.
                    shouldStoreMessage = response.StatusCode >= HttpStatusCode.InternalServerError;
                }
                else
                {
                    // Clear the breadcrumbs after a successful send.
                    _breadcrumbs.Clear();
                    _raygunLogger?.Debug("[RaygunBlazorClient] Request sent to Raygun: " + response.StatusCode);

                    // Message was sent successfully.
                    return true;
                }
            }
            catch (Exception ex)
            {
                _raygunLogger?.Error(
                    "[RaygunBlazorClient] Failed to send request to Raygun: " + ex.Message);

                // Could be caused by networking error, so store the message for later.
                shouldStoreMessage = true;
            }

            if (shouldStoreMessage && useOfflineStore)
            {
                if (_offlineStore != null)
                {
                    // Store the message for later.
                    await _offlineStore.Save(request, cancellationToken).ConfigureAwait(false);

                    // Because the request is now stored to send later, we can clear the breadcrumbs.
                    _breadcrumbs.Clear();

                    _raygunLogger?.Debug("[RaygunBlazorClient] Request stored for offline processing.");
                }
            }

            // If we get here, the message was not sent successfully.
            return false;
        }

        /// <summary>
        /// Processes unhandled exceptions from JavaScript and reports them to Raygun.
        /// </summary>
        /// <param name="errorEvent">The <see cref="ErrorEvent" /> passed up from the DOM.</param>
        /// <remarks>
        /// This method signature is passed to the <see cref="RaygunBrowserInterop" /> during initialization.
        /// </remarks>
        private async Task OnUnhandledJsException(ErrorEvent errorEvent)
        {
            _raygunLogger?.Verbose("[RaygunBlazorClient] Unhandled JavaScript exception caught: " + errorEvent);
            WebIDLException? exception = await errorEvent.GetErrorAsExceptionAsync();
            if (exception is null)
            {
                _raygunLogger?.Warning("[RaygunBlazorClient] Failed to convert JavaScript error to WebIDLException.");
                return;
            }

            await RecordExceptionAsync(exception, null, ["UnhandledException", "Blazor", "JavaScript"]);
        }

        #endregion
    }
}