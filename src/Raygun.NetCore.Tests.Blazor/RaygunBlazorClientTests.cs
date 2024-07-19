using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Raygun.NetCore.Blazor;
using System;
using System.Threading.Tasks;
using Bunit;
using CloudNimble.Breakdance.Blazor;
using Raygun.Blazor;
using Raygun.Blazor.Extensions;
using Raygun.Blazor.Models;

namespace Raygun.NetCore.Tests.Blazor
{
    /// <summary>
    /// Tests the functionality of the code that registers Raygun resources with the DI container.
    /// </summary>
    [TestClass]
    public class ServiceCollectionExtensionsTests : BlazorBreakdanceTestBase
    {
        #region Test Lifecycle

        [TestInitialize]
        public void Setup()
        {
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

                services.AddRaygunBlazor(context.Configuration);
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
        }

        #endregion
    }
}