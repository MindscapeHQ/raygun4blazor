using CloudNimble.Breakdance.Assemblies;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Raygun.NetCore.Blazor;
using Raygun.NetCore.Blazor.Models;
using System;
using System.Threading.Tasks;

namespace Raygun.NetCore.Tests.Blazor
{

    /// <summary>
    /// Tests the functionality of the code that registers Raygun resources with the DI container.
    /// </summary>
    [TestClass]
    public class IServiceCollectionExtensionsTests : BreakdanceTestBase
    {

        #region Test Lifecycle

        [TestInitialize]
        public void Setup()
        {
            TestHostBuilder.ConfigureServices((context, services) =>
            {
                services.AddRaygunBlazor(context.Configuration);
            });
            TestSetup();
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
