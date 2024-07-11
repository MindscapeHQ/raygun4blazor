using KristofferStrube.Blazor.WebIDL.Exceptions;
using KristofferStrube.Blazor.Window;
using Microsoft.Extensions.Options;
using Raygun.NetCore.Blazor.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace Raygun.NetCore.Blazor
{

    /// <summary>
    /// A Raygun client designed specifically for Blazor applications.
    /// </summary>
    public class RaygunBlazorClient
    {

        #region Private Members

        private readonly List<BreadcrumbDetails> _breadcrumbs;
        private readonly RaygunBrowserInterop _browserInterop;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly JsonSerializerOptions _jsonOptions;
        private readonly IRaygunOfflineStore _offlineStore;
        private readonly IRaygunQueueManager _queueManager;
        private readonly RaygunSettings _raygunSettings;
        private readonly IRaygunUserManager _userManager;

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new instance of the <see cref="RaygunBlazorClient" /> class.
        /// </summary>
        /// <param name="raygunSettings">The <see cref="RaygunSettings" /> injected into the DI container by calling services.AddRaygunBlazor().</param>
        /// <param name="httpClientFactory">The <see cref="IHttpClientFactory" /> injected into the DI container by calling services.AddRaygunBlazor().</param>
        /// <param name="browserInterop"></param>
        /// <remarks>
        /// You should not usually create a new instance yourself, instead get a usable instance from the DI container by injecting it into the Blazor page directly.
        /// </remarks>
        public RaygunBlazorClient(IOptions<RaygunSettings> raygunSettings, IHttpClientFactory httpClientFactory, RaygunBrowserInterop browserInterop/*, IRaygunQueueManager queueManager, IRaygunUserManager userManager, IRaygunOfflineStore offlineStore*/)
        {
            // RWM: We do this first because there is no point consuming CPU cycles setting properties if the API key is missing.
            _raygunSettings = raygunSettings.Value;
            if (string.IsNullOrWhiteSpace(_raygunSettings.ApiKey))
            {
                throw new ArgumentNullException("RaygunSettings.ApiKey", "A Raygun API Key was not provided. Please check your settings and try again.");
            }

            _breadcrumbs = [];
            _browserInterop = browserInterop;
            _httpClientFactory = httpClientFactory;
            _jsonOptions = new JsonSerializerOptions
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            };
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
        public void RecordBreadcrumb(string message, BreadcrumbType breadcrumbType = BreadcrumbType.Manual, string category = null, 
            Dictionary<string, object> customData = null, string platform = "DotNet")
        { 
            _breadcrumbs.Add(new BreadcrumbDetails(message, breadcrumbType, category, customData, platform));
        }


        /// <summary>
        /// Queues an exception to be sent to Raygun.
        /// </summary>
        /// <param name="ex">The exception to be sent back to Raygun.</param>
        /// <param name="tags">User-specified tags that should be applied to the error.</param>
        /// <param name="addUserDetails">Specifies whether or not details from the current user should be added to the report.</param>
        /// <param name="userCustomData">Any custom data that you you like sent with the report to assist with troubleshooting.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken" /> to allow you to cancel the current request, if necessary.</param>
        /// <returns></returns>
        /// <code>
        /// TBD
        /// </code>
        public async Task RecordExceptionAsync(Exception ex, List<string>? tags = null, bool addUserDetails = false, Dictionary<string, string>? userCustomData = null,
            CancellationToken cancellationToken = default)
        {
            await InitializeAsync();

            var appVersion = _raygunSettings.ApplicationVersion ??
                System.Reflection.Assembly.GetEntryAssembly().GetName().Version.ToString() ??
                null;

            var userDetails = addUserDetails ? new UserDetails() : null;

            var request = new RaygunRequest
            {
                Details = new EventDetails(appVersion)
                {
                    Breadcrumbs = _breadcrumbs.ToList(),
                    Environment = await _browserInterop.GetBrowserEnvironment(),
                    Error = new ErrorDetails(ex),
                    Tags = tags,
                    User = userDetails,
                    UserCustomData = userCustomData,
                }
            };
            _breadcrumbs.Clear();

            // TODO: RWM: Queue the request to be sent out-of-band.
            //queueManager.Enqueue(request);

            // RWM: This is temporary for now to ensure the request is properly created.
            var client = _httpClientFactory.CreateClient("Raygun");
            var response = await client.PostAsJsonAsync(RaygunSettings.EntriesEndpoint, request, _jsonOptions, cancellationToken);

            // TODO: RWM: Do something if the request fails:
            //            202 OK - Message accepted.
            //            400 Bad message - could not parse the provided JSON. Check all fields are present, especially both occurredOn (ISO 8601 DateTime) and details { } at the top level.
            //            403 Invalid API Key - The value specified in the header X-ApiKey did not match with a user.
            //            413 Request entity too large - The maximum size of a JSON payload is 128KB.
            //            429 Too Many Requests - Plan limit exceeded for month or plan expired
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
        /// Processes unhandled exceptions from JavaScript and reports them to Raygun.
        /// </summary>
        /// <param name="errorEvent">The <see cref="ErrorEvent" /> passed up from the DOM.</param>
        /// <remarks>
        /// This method signature is passed to the <see cref="RaygunBrowserInterop" /> during initialization.
        /// </remarks>
        internal async Task OnUnhandledJsException(ErrorEvent errorEvent)
        {
            WebIDLException? exception = await errorEvent.GetErrorAsExceptionAsync();
            if (exception is null) return;
            await RecordExceptionAsync(exception, ["UnhandledException", "Blazor", "JavaScript"]);
        }

        #endregion

    }

}
