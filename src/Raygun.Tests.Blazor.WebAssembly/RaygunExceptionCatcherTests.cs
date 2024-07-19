using Bunit;
using CloudNimble.Breakdance.Blazor;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Raygun.Blazor.Extensions;

namespace Raygun.Tests.Blazor.WebAssembly
{
    /// <summary>
    /// 
    /// </summary>
    [TestClass]
    public class RaygunExceptionCatcherTests : BlazorBreakdanceTestBase
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
    }
}