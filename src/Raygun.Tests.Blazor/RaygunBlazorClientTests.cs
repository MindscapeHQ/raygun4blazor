using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Bunit;
using CloudNimble.Breakdance.Blazor;
using FluentAssertions;
using KristofferStrube.Blazor.Window;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MockHttp;
using Raygun.Blazor;
using Raygun.Blazor.Interfaces;
using Raygun.Blazor.Models;
using Raygun.Blazor.Offline;
using Raygun.Blazor.Offline.SendStrategy;

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
        private TestSendStrategy _testSendStrategy = null!;
        private RaygunLocalOfflineStore _raygunOfflineStore = null!;

        private readonly JsonSerializerOptions _jsonSerializerOptions =
            new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, };

        [TestInitialize]
        public void Setup()
        {
            _mockHttp = new MockHttpHandler();
            _httpClient = new HttpClient(_mockHttp);

            // Raygun Offline Store section
            // See RaygunOfflineStoreTests.cs
            var tempDirName = "TestDirectory";
            IOptions<RaygunSettings> options = Options.Create(new RaygunSettings
            {
                DirectoryName = tempDirName,
            });
            var tempDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                tempDirName);
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, true);
            }

            _testSendStrategy = new TestSendStrategy();
            _raygunOfflineStore = new RaygunLocalOfflineStore(_testSendStrategy, options);

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

                // Set up a local offline store
                services.AddSingleton<IRaygunOfflineStore>(_raygunOfflineStore);

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

            // Check Environment details
            raygunMsg.Details.Environment.Architecture.Should().NotBeEmpty();
            raygunMsg.Details.Environment.Cpu.Should().NotBeEmpty();
            raygunMsg.Details.Environment.DeviceName.Should().NotBeEmpty();
            raygunMsg.Details.Environment.OSVersion.Should().NotBeEmpty();
            raygunMsg.Details.Environment.ProcessorCount.Should().NotBeNull();
            raygunMsg.Details.Environment.UtcOffset.Should().NotBeNull();
            raygunMsg.Details.Environment.TotalPhysicalMemory.Should().NotBeNull();
            raygunMsg.Details.Environment.TotalVirtualMemory.Should().NotBeNull();

            // Check Browser environment details from UserCustomData
            var browserEnvJson = raygunMsg.Details.UserCustomData["BrowserEnvironment"];
            browserEnvJson.Should().NotBeNull();
            var browserEnv = JsonSerializer.Deserialize<EnvironmentDetails>((JsonElement)browserEnvJson, _jsonSerializerOptions)!;
            browserEnv.BrowserName.Should().Be("Firefox");
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

            raygunClient.RecordBreadcrumb("About to send the test exception", BreadcrumbType.Manual,
                "Unit Tests", new Dictionary<string, object>(), "DotNet", BreadcrumbLevel.Error);

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
            raygunMsg.Details.Breadcrumbs[0].Level.Should().Be(BreadcrumbLevel.Error);
        }

        /// <summary>
        /// OnBeforeSend can modify the request
        /// </summary>
        [TestMethod]
        public async Task RaygunBlazorClient_BasicException_OnBeforeSend_ShouldModifyAndSend()
        {
            var raygunClient = TestHost.Services.GetService<RaygunBlazorClient>();
            raygunClient.Should().NotBeNull();

            // Add a handler that changes the error message
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

        /// <summary>
        /// Store failed to send report into offline store, then attempt to send it again
        /// </summary>
        [TestMethod]
        public async Task RaygunBlazorClient_StoreFailedReportAndSendAfter()
        {
            // Request will fail with a 500 error, so it will be stored
            _mockHttp.When(match => match.Method(HttpMethod.Post).RequestUri("https://api.raygun.com/entries"))
                .Respond(x => x.StatusCode(HttpStatusCode.InternalServerError)).Verifiable();

            var raygunClient = TestHost.Services.GetService<RaygunBlazorClient>();
            raygunClient.Should().NotBeNull();

            Func<Task> recordException = async () => await raygunClient.RecordExceptionAsync(new Exception("Test"));
            await recordException.Should().NotThrowAsync();

            await Task.Delay(500);

            // Ensure the request was made 
            _mockHttp.InvokedRequests.Should().HaveCount(1);

            // Trigger sending stored reports
            await _testSendStrategy.SendAll();
            await Task.Delay(500);

            // A new request is made
            _mockHttp.InvokedRequests.Should().HaveCount(2);

            // Obtain requested data
            var request = _mockHttp.InvokedRequests[1].Request;
            var content = await request.Content?.ReadAsStringAsync()!;

            // Deserialize request
            var raygunMsg = JsonSerializer.Deserialize<RaygunRequest>(content, _jsonSerializerOptions)!;

            // Check error details
            raygunMsg.Details.Error.ClassName.Should().Be("System.Exception");
            raygunMsg.Details.Error.Message.Should().Be("Test");
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