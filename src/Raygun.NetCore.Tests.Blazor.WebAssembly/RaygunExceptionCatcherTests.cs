using CloudNimble.Breakdance.Blazor;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Raygun4Net.Tests.Blazor.WebAssembly
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
            TestSetup();
        }

        [TestCleanup]
        public void TearDown() => TestTearDown();

        #endregion



    }

}
