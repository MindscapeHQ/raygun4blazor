using System;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Bunit;
using Bunit.TestDoubles;
using CloudNimble.Breakdance.Blazor;
using FluentAssertions;
using KristofferStrube.Blazor.Window;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MockHttp;
using Raygun.Blazor;
using Raygun.Blazor.Extensions;
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
                services.AddSingleton<IRaygunUserManager>(new FakeRaygunUserManager());
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

            // Check user details from FakeRaygunUserManager
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
            // TODO: How to verify that the user data was sent? mocked HTTP Client?

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

    class FakeRaygunUserManager : IRaygunUserManager
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