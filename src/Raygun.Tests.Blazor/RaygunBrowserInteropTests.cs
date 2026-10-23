using System.Threading.Tasks;
using Bunit;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Raygun.Blazor;
using Raygun.Blazor.Models;

namespace Raygun.Tests.Blazor;

[TestClass]
public class RaygunBrowserInteropTests
{
    private BunitJSInterop _jsInterop = null!;

    [TestInitialize]
    public void Setup()
    {
        _jsInterop = new BunitJSInterop { Mode = JSRuntimeMode.Loose };
        _jsInterop.Setup<BrowserSpecs>("getBrowserSpecs").SetResult(new BrowserSpecs
        {
            UserAgent = "Mozilla/5.0 (Windows NT 6.1; Win64; x64; rv:47.0) Gecko/20100101 Firefox/47.0",
        });
    }

    [TestMethod]
    public async Task InitializeAsync_CatchUnhandledExceptions_AttachesErrorListener()
    {
        await CreateInterop(catchUnhandledExceptions: true).InitializeAsync(null, null);

        _jsInterop.VerifyInvoke("window.raygunBlazor.initialize");
        _jsInterop.VerifyInvoke("window.raygunBlazor.attachErrorListener");
    }

    [TestMethod]
    public async Task InitializeAsync_NotCatchUnhandledExceptions_DoesNotAttachErrorListener()
    {
        await CreateInterop(catchUnhandledExceptions: false).InitializeAsync(null, null);

        // initialize must still run so manual JS recordException / recordBreadcrumb keep working.
        _jsInterop.VerifyInvoke("window.raygunBlazor.initialize");
        _jsInterop.VerifyNotInvoke("window.raygunBlazor.attachErrorListener");
    }

    private RaygunBrowserInterop CreateInterop(bool catchUnhandledExceptions)
    {
        var settings = Options.Create(new RaygunSettings { CatchUnhandledExceptions = catchUnhandledExceptions });
        return new RaygunBrowserInterop(_jsInterop.JSRuntime, settings);
    }
}
