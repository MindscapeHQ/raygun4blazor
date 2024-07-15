using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Raygun.NetCore.Blazor;
using System.Net.Http;
using Bunit;
using CloudNimble.Breakdance.Blazor;

namespace Raygun.NetCore.Tests.Blazor.Extensions
{
    /// <summary>
    /// 
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
                services.AddRaygunBlazor(context.Configuration);
            });
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
        public void AddRaygunBlazor_ShouldRegisterDependencies()
        {
            var settings = TestHost.Services.GetService<IOptions<RaygunSettings>>();
            settings.Should().NotBeNull();
            settings.Value.Should().NotBeNull();
            settings.Value.ApiKey.Should().NotBeNullOrWhiteSpace();
            TestHost.Services.GetService<IHttpClientFactory>().Should().NotBeNull();
            TestHost.Services.GetService<RaygunBlazorClient>().Should().NotBeNull();
        }

        // TODO: RWM: What happens if the ApiKey is null or whitespace?

        #endregion
    }
}