using Bunit;
using CloudNimble.Breakdance.Blazor;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net;
using System.Text.Json;
using KristofferStrube.Blazor.Window;
using Raygun.Blazor;
using MockHttp;
using Raygun.Tests.Blazor.Server.Components;
using Raygun.Blazor.Models;
using FluentAssertions;

namespace Raygun.Tests.Blazor.Server
{
    /// <summary>
    /// 
    /// </summary>
    [TestClass]
    public class RaygunExceptionCatcherTests : BlazorBreakdanceTestBase
    {
        #region Test Lifecycle

        private MockHttpHandler _mockHttp = null!;
        private HttpClient _httpClient = null!;
        private HttpRequestMessage? _lastRequest;

        private readonly JsonSerializerOptions _jsonSerializerOptions =
            new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, };

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

        #endregion

        #region Tests
        [TestMethod]
        public async Task RaygunExceptionCatcher_WhenExceptionIsThrown_ExceptionIsCaught()
        {
            // Arrange
            var cut = BUnitTestContext.RenderComponent<App>();

            Console.WriteLine("Initialized");

            // Act
            cut.Find("button").Click();

            // Obtain requested data
            var request = _mockHttp.InvokedRequests[0].Request;
            var content = await request.Content?.ReadAsStringAsync()!;

            // Deserialize request
            var raygunMsg = JsonSerializer.Deserialize<RaygunRequest>(content, _jsonSerializerOptions)!;

            // Check error details
            raygunMsg.Details!.Error!.Message.Should().Be("Captured error!");
        }
        #endregion
    }
}