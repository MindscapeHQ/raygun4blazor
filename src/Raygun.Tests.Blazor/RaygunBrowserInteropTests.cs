using System;
using System.Collections.Generic;
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

    [TestMethod]
    public async Task RecordJsException_ForwardsJsUnhandledExceptionWithPayloadDetails()
    {
        var calls = new List<(Exception Exception, UserDetails? User, List<string>? Tags, Dictionary<string, object>? CustomData)>();
        var interop = CreateInterop(catchUnhandledExceptions: true);
        await interop.InitializeAsync(null, (exception, user, tags, customData, _) =>
        {
            calls.Add((exception, user, tags, customData));
            return Task.CompletedTask;
        });
        var payload = new JsErrorPayload
        {
            Name = "TypeError",
            Message = "x is undefined",
            Stack = "TypeError: x is undefined\n    at foo (app.js:1:1)",
        };
        var tags = new List<string> { "UnhandledException" };
        var customData = new Dictionary<string, object> { ["key"] = "value" };

        await interop.RecordJsException(payload, tags, customData);

        calls.Should().ContainSingle();
        var call = calls[0];
        var exception = call.Exception.Should().BeOfType<JsUnhandledException>().Subject;
        exception.JsName.Should().Be("TypeError");
        exception.Message.Should().Be("x is undefined");
        exception.StackTrace.Should().Be(payload.Stack);
        call.User.Should().BeNull();
        call.Tags.Should().BeSameAs(tags);
        call.CustomData.Should().BeSameAs(customData);
    }

    [TestMethod]
    public async Task RecordJsException_EmptyPayload_ForwardsExceptionWithNullNameAndStack()
    {
        Exception? captured = null;
        var interop = CreateInterop(catchUnhandledExceptions: true);
        await interop.InitializeAsync(null, (exception, _, _, _, _) =>
        {
            captured = exception;
            return Task.CompletedTask;
        });

        // Safari and cross-origin scripts dispatch ErrorEvents with no error object, so every field can be null.
        await interop.RecordJsException(new JsErrorPayload());

        var exception = captured.Should().BeOfType<JsUnhandledException>().Subject;
        exception.JsName.Should().BeNull();
        exception.StackTrace.Should().BeNull();
    }

    private RaygunBrowserInterop CreateInterop(bool catchUnhandledExceptions)
    {
        var settings = Options.Create(new RaygunSettings { CatchUnhandledExceptions = catchUnhandledExceptions });
        return new RaygunBrowserInterop(_jsInterop.JSRuntime, settings);
    }
}
