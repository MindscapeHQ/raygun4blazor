using CloudNimble.Breakdance.Blazor;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Raygun.NetCore.Tests.Blazor.WebAssembly
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
                
                var jsRuntimeMock = new Mock<IJSRuntime>();
                services.AddSingleton(jsRuntimeMock.Object);
                services.AddRaygunBlazor(context.Configuration);
            });
            TestSetup();
        }

        [TestCleanup]
        public void TearDown() => TestTearDown();

        #endregion



    }

}
