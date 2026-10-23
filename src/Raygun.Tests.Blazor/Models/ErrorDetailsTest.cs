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
