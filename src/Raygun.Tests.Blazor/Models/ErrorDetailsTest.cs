using System;
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
        var stacktrace = "causeErrors@http://localhost:5010/myfunctions.js:7:9\n"
            + "window.onmessage@http://localhost:5010/:21:17\n"
            + "EventHandlerNonNull*@http://localhost:5010/:18:9";
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
    }

    /// <summary>
    /// Regression test for issue #96: when the browser dispatches an ErrorEvent
    /// with no backing Error (e.g. iOS/Safari, cross-origin scripts), Raygun
    /// must still produce a usable ErrorDetails without throwing.
    /// </summary>
    [TestMethod]
    public void ErrorDetails_NewInstance_JavaScriptException_NullStackAndName()
    {
        var exception = new JsUnhandledException(name: null, message: null, stack: null);

        Action build = () => _ = new ErrorDetails(exception);

        build.Should().NotThrow();
        var errorDetails = new ErrorDetails(exception);
        errorDetails.Message.Should().Be("JavaScript error");
        errorDetails.StackTrace.Should().BeNull();
    }
}
