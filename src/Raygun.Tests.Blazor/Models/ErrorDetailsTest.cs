using System;
using System.Diagnostics;
using System.Linq;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Raygun.Blazor;
using Raygun.Blazor.Models;

namespace Raygun.Tests.Blazor.Models;

[TestClass]
public class ErrorDetailsTest
{
    [TestMethod]
    public void ErrorDetails_NewInstance_DotnetException()
    {
        // Define Dotnet exception
        var exception = new Exception("Test");

        // Parse exception
        var errorDetails = new ErrorDetails(exception);

        // Check details
        errorDetails.ClassName.Should().Be("System.Exception");
        errorDetails.Message.Should().Be("Test");
    }

    [TestMethod]
    public void ErrorDetails_NewInstance_JavaScriptException()
    {
        // Define JavaScript exception
        var stacktrace = @"causeErrors@http://localhost:5010/myfunctions.js:7:9 
        window.onmessage@http://localhost:5010/:21:17
        EventHandlerNonNull*@http://localhost:5010/:18:9";
        var exception = new JsUnhandledException("TypeError", "Test", stacktrace);

        // Parse exception
        var errorDetails = new ErrorDetails(exception);

        // Check details — ClassName surfaces the JS error name (e.g. "TypeError"), not a .NET type.
        errorDetails.ClassName.Should().Be("TypeError");
        errorDetails.Message.Should().Be("Test");

        // Check parsed stack trace
        errorDetails.StackTrace!.Count.Should().Be(3);
        var traceDetails = errorDetails.StackTrace!.First();
        traceDetails.ColumnNumber.Should().Be(9);
        traceDetails.FileName.Should().Be("http://localhost:5010/myfunctions.js");
        traceDetails.LineNumber.Should().Be(7);
        traceDetails.MethodName.Should().Be("causeErrors");

        // Indented lines are trimmed before parsing.
        errorDetails.StackTrace![1].MethodName.Should().Be("window.onmessage");
    }

    /// <summary>
    /// Chrome and Edge start the stack with a "name: message" header line, which is not a frame.
    /// </summary>
    [TestMethod]
    public void ErrorDetails_NewInstance_JavaScriptException_ChromeStack()
    {
        // Payload observed in issue #126.
        var exception = new JsUnhandledException("TypeError", "x is undefined",
            "TypeError: x is undefined\n    at http://localhost:5010/smoke.js:24:64");

        var errorDetails = new ErrorDetails(exception);

        errorDetails.StackTrace.Should().ContainSingle();
        var frame = errorDetails.StackTrace![0];
        frame.MethodName.Should().Be("?");
        frame.FileName.Should().Be("http://localhost:5010/smoke.js");
        frame.LineNumber.Should().Be(24);
        frame.ColumnNumber.Should().Be(64);
    }

    [TestMethod]
    public void ErrorDetails_NewInstance_JavaScriptException_SafariAnonymousStack()
    {
        // Payload observed in issue #126 (Safari on iOS and MAUI's WKWebView).
        var exception = new JsUnhandledException("TypeError", "x is undefined",
            "@http://localhost:5010/smoke.js:24:77");

        var errorDetails = new ErrorDetails(exception);

        errorDetails.StackTrace.Should().ContainSingle();
        var frame = errorDetails.StackTrace![0];
        frame.MethodName.Should().Be("?");
        frame.FileName.Should().Be("http://localhost:5010/smoke.js");
        frame.LineNumber.Should().Be(24);
        frame.ColumnNumber.Should().Be(77);
    }

    /// <summary>
    /// When a .NET call from JS fails, Blazor puts the whole .NET exception in the JS error's message,
    /// so the header spans several lines and contains lines that look like frames.
    /// </summary>
    [TestMethod]
    public void ErrorDetails_NewInstance_JavaScriptException_MultiLineHeaderRemoved()
    {
        const string message = "System.InvalidOperationException: boom\n   at MyApp.Pages.Home.Throw() in /src/Home.razor:line 12";
        var exception = new JsUnhandledException("Error", message,
            $"Error: {message}\n    at http://localhost:5010/app.js:1:2");

        var errorDetails = new ErrorDetails(exception);

        errorDetails.StackTrace.Should().ContainSingle();
        errorDetails.StackTrace![0].FileName.Should().Be("http://localhost:5010/app.js");
    }

    /// <summary>
    /// A V8 stack with no frames (e.g. Error.stackTraceLimit = 0) is only the header. The header must
    /// not be parsed as a frame, even when the message contains "@". Like Error.prototype.toString(),
    /// the header is the name alone when the message is empty, and the message alone when the name is.
    /// </summary>
    [TestMethod]
    [DataRow("Error", "user a@b.com not found", "Error: user a@b.com not found")]
    [DataRow("Error", "", "Error")]
    [DataRow("", "boom", "boom")]
    public void ErrorDetails_NewInstance_JavaScriptException_HeaderOnlyStack(string name, string message, string stack)
    {
        var exception = new JsUnhandledException(name, message, stack);

        var errorDetails = new ErrorDetails(exception);

        errorDetails.StackTrace.Should().BeNull();
    }

    /// <summary>
    /// Chrome fixes the message in the header when the error is created, so a message changed before
    /// the error is thrown no longer matches it. The header is still not a frame.
    /// </summary>
    [TestMethod]
    public void ErrorDetails_NewInstance_JavaScriptException_ChromeHeaderWithChangedMessage()
    {
        // Captured in Chrome: var e = new Error('save failed'); e.message = 'user a@b.com: ' + e.message; throw e;
        var exception = new JsUnhandledException("Error", "user a@b.com: save failed",
            "Error: save failed\n    at saveUser (http://localhost:5010/app.js:3:9)");

        var errorDetails = new ErrorDetails(exception);

        errorDetails.StackTrace.Should().ContainSingle();
        errorDetails.StackTrace![0].MethodName.Should().Be("saveUser");
    }

    /// <summary>
    /// When a changed message stops the header matching, the header is everything before the first V8
    /// frame line. The .NET stack lines in a Blazor message start with three spaces and are not frames.
    /// </summary>
    [TestMethod]
    public void ErrorDetails_NewInstance_JavaScriptException_ChangedMultiLineHeaderRemoved()
    {
        const string original = "System.InvalidOperationException: boom\n   at MyApp.Pages.Home.Throw() in /src/Home.razor:line 12";
        var exception = new JsUnhandledException("Error", "Render failed: " + original,
            $"Error: {original}\n    at http://localhost:5010/app.js:1:2");

        var errorDetails = new ErrorDetails(exception);

        errorDetails.StackTrace.Should().ContainSingle();
        errorDetails.StackTrace![0].FileName.Should().Be("http://localhost:5010/app.js");
    }

    /// <summary>
    /// A Safari stack has no header, so a first frame that merely starts with the error's name is kept.
    /// </summary>
    [TestMethod]
    public void ErrorDetails_NewInstance_JavaScriptException_SafariFrameStartingWithErrorName()
    {
        // Safari: new Error() thrown from a function named ErrorBoundary.
        var exception = new JsUnhandledException("Error", "", "ErrorBoundary@http://localhost:5010/app.js:1:2");

        var errorDetails = new ErrorDetails(exception);

        errorDetails.StackTrace.Should().ContainSingle();
        errorDetails.StackTrace![0].MethodName.Should().Be("ErrorBoundary");
    }

    /// <summary>
    /// When the browser dispatches an ErrorEvent with no backing Error (e.g. iOS/Safari,
    /// cross-origin scripts), the JS error carries no name or stack. ErrorDetails must
    /// still be built from what remains.
    /// </summary>
    [TestMethod]
    public void ErrorDetails_NewInstance_JavaScriptException_NullStackAndName()
    {
        // What Raygun.Blazor.ts sends when event.error is null: only the event's message survives.
        var exception = new JsUnhandledException(name: null, message: "Script error.", stack: null);

        Action build = () => _ = new ErrorDetails(exception);

        build.Should().NotThrow();
        var errorDetails = new ErrorDetails(exception);
        errorDetails.ClassName.Should().Be(typeof(JsUnhandledException).FullName);
        errorDetails.Message.Should().Be("Script error.");
        errorDetails.StackTrace.Should().BeNull();
    }
}
