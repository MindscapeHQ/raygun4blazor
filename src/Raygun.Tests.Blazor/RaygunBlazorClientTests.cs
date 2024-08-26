using System;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Bunit;
using CloudNimble.Breakdance.Blazor;
using FluentAssertions;
using KristofferStrube.Blazor.Window;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MockHttp;
using Raygun.Blazor;
using Raygun.Blazor.Interfaces;
using Raygun.Blazor.Models;

namespace Raygun.Tests.Blazor
{
    /// <summary>
    /// Tests the functionality of the code that registers Raygun resources with the DI container.
    /// </summary>
    [TestClass]
    public class ServiceCollectionExtensionsTests : BlazorBreakdanceTestBase
    {
        #region Test Lifecycle

        private MockHttpHandler _mockHttp = null!;
        private HttpClient _httpClient = null!;
        private readonly JsonSerializerOptions _jsonSerializerOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, };

        [TestInitialize]
        public void Setup()
        {
            _mockHttp = new MockHttpHandler();
            _httpClient = new HttpClient(_mockHttp);

            TestHostBuilder.ConfigureServices((context, services) =>
            {
                // Prepare fakes for BrowserSpecs and BrowserStats
                var browserSpecs = new BrowserSpecs();
                var browserStats = new BrowserStats();
                browserSpecs.UserAgent =
                    "Mozilla/5.0 (Windows NT 6.1; Win64; x64; rv:47.0) Gecko/20100101 Firefox/47.0";

                // BlazorBreakdanceTestBase exposes bunit JSInterop
                BUnitTestContext.JSInterop.Setup<BrowserSpecs>("getBrowserSpecs").SetResult(browserSpecs);
                BUnitTestContext.JSInterop.Setup<BrowserStats>("getBrowserStats").SetResult(browserStats);

                // Create RaygunBlazorClient with mocked HttpClient
                services.Configure<RaygunSettings>(context.Configuration.GetSection("Raygun"));
                services.AddScoped<RaygunBrowserInterop>();
                services.AddWindowService();
                services.AddSingleton<IHttpClientFactory>(new MockHttpClientFactory(_httpClient));
                services.AddSingleton<IRaygunUserProvider>(new FakeRaygunUserProvider());
                services.AddScoped<RaygunBlazorClient>();

                // Mock entries requests
                _mockHttp.When(match => match.Method(HttpMethod.Post).RequestUri("https://api.raygun.com/entries"))
                         .Respond(x =>
                         {
                             x.Body("OK");
                             x.StatusCode(HttpStatusCode.Accepted);
                         }).Verifiable();

            });

            // See: https://bunit.dev/docs/test-doubles/emulating-ijsruntime.html
            TestSetup(JSRuntimeMode.Loose);
        }

        [TestCleanup]
        public void TearDown() => TestTearDown();

        #endregion

        #region Test Methods

        /// <summary>
        /// Verifies that all dependencies are properly registered and configured.
        /// </summary>
        [TestMethod]
        public async Task RaygunBlazorClient_BasicException_ShouldSend()
        {
            var raygunClient = TestHost.Services.GetService<RaygunBlazorClient>();
            raygunClient.Should().NotBeNull();

            Func<Task> recordException = async () => await raygunClient.RecordExceptionAsync(new Exception("Test"));
            await recordException.Should().NotThrowAsync();

            await Task.Delay(500);

            // Obtain requested data
            var request = _mockHttp.InvokedRequests[0].Request;
            var content = await request.Content?.ReadAsStringAsync()!;

            // Deserialize request
            var raygunMsg = JsonSerializer.Deserialize<RaygunRequest>(content, _jsonSerializerOptions)!;

            // Check error details
            raygunMsg.Details.Error.ClassName.Should().Be("System.Exception");
            raygunMsg.Details.Error.Message.Should().Be("Test");

            // Check user details from FakeRaygunUserProvider
            raygunMsg.Details.User.FullName.Should().Be("Manager User");
            raygunMsg.Details.User.Email.Should().Be("manager@example.com");
        }

        /// <summary>
        /// 
        /// </summary>
        [TestMethod]
        public async Task RaygunBlazorClient_BasicException_WithUserData_ShouldSend()
        {
            var raygunClient = TestHost.Services.GetService<RaygunBlazorClient>();
            raygunClient.Should().NotBeNull();

            var userDetails = new UserDetails() { FullName = "Custom Test User", Email = "custom@example.com" };
            Func<Task> recordException = async () => await raygunClient.RecordExceptionAsync(new Exception("Test"),
                userDetails);
            await recordException.Should().NotThrowAsync();

            await Task.Delay(500);

            // Obtain requested data
            var request = _mockHttp.InvokedRequests[0].Request;
            var content = await request.Content?.ReadAsStringAsync()!;

            // Deserialize request
            var raygunMsg = JsonSerializer.Deserialize<RaygunRequest>(content, _jsonSerializerOptions)!;

            // Check user details from userDetails argument in RecordExceptionAsync
            raygunMsg.Details.User.FullName.Should().Be("Custom Test User");
            raygunMsg.Details.User.Email.Should().Be("custom@example.com");

        }

        /// <summary>
        /// Verifies that all dependencies are properly registered and configured.
        /// </summary>
        [TestMethod]
        public async Task RaygunBlazorClient_BasicException_WithBreadcrumbs_ShouldSend()
        {
            var raygunClient = TestHost.Services.GetService<RaygunBlazorClient>();
            raygunClient.Should().NotBeNull();

            raygunClient.RecordBreadcrumb("About to send the test exception", BreadcrumbType.Manual, "Unit Tests");

            Func<Task> recordException = async () => await raygunClient.RecordExceptionAsync(new Exception("Test"));
            await recordException.Should().NotThrowAsync();

            await Task.Delay(500);

            // Obtain requested data
            var request = _mockHttp.InvokedRequests[0].Request;
            var content = await request.Content?.ReadAsStringAsync()!;

            // Deserialize request
            var raygunMsg = JsonSerializer.Deserialize<RaygunRequest>(content, _jsonSerializerOptions)!;

            // Check user details
            raygunMsg.Details.Breadcrumbs[0].Message.Should().Be("About to send the test exception");
            raygunMsg.Details.Breadcrumbs[0].Type.Should().Be(BreadcrumbType.Manual);
            raygunMsg.Details.Breadcrumbs[0].Category.Should().Be("Unit Tests");
        }

        /// <summary>
        /// OnBeforeSend can modify the request
        /// </summary>
        [TestMethod]
        public async Task RaygunBlazorClient_BasicException_OnBeforeSend_ShouldModifyAndSend()
        {
            var raygunClient = TestHost.Services.GetService<RaygunBlazorClient>();
            raygunClient.Should().NotBeNull();

            // Add a handler that cancels the send operation
            raygunClient.OnBeforeSend += (sender, args) => args.Request.Details.Error.Message = "MODIFIED";

            Func<Task> recordException = async () => await raygunClient.RecordExceptionAsync(new Exception("Test"));
            await recordException.Should().NotThrowAsync();

            await Task.Delay(500);

            // Obtain requested data
            var request = _mockHttp.InvokedRequests[0].Request;
            var content = await request.Content?.ReadAsStringAsync()!;

            // Deserialize request
            var raygunMsg = JsonSerializer.Deserialize<RaygunRequest>(content, _jsonSerializerOptions)!;

            // Check error details
            raygunMsg.Details.Error.Message.Should().Be("MODIFIED");
        }

        /// <summary>
        /// OnBeforeSend can cancel the request
        /// </summary>
        [TestMethod]
        public async Task RaygunBlazorClient_BasicException_OnBeforeSend_ShouldCancel()
        {
            var raygunClient = TestHost.Services.GetService<RaygunBlazorClient>();
            raygunClient.Should().NotBeNull();

            // Add a handler that cancels the send operation
            raygunClient.OnBeforeSend += (sender, args) => args.Cancel = true;

            Func<Task> recordException = async () => await raygunClient.RecordExceptionAsync(new Exception("Test"));
            await recordException.Should().NotThrowAsync();

            await Task.Delay(500);

            // Ensure no requests were made
            _mockHttp.InvokedRequests.Should().BeEmpty();
        }

        #endregion
    }

    class MockHttpClientFactory : IHttpClientFactory
    {
        private HttpClient _httpClient = null!;

        public MockHttpClientFactory(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri("https://api.raygun.com");
        }

        public HttpClient CreateClient(string name)
        {
            return _httpClient;
        }
    }

    class FakeRaygunUserProvider : IRaygunUserProvider
    {
        public Task<UserDetails> GetCurrentUser()
        {
            return Task.FromResult(new UserDetails()
            {
                FullName = "Manager User",
                Email = "manager@example.com",
            });
        }
    }
}