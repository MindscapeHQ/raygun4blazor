using System;
using System.Diagnostics;
using System.Linq;
using FluentAssertions;
using KristofferStrube.Blazor.WebIDL.Exceptions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
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
        var exception = new WebIDLException("Test", stacktrace, null!);

        // Parse exception
        var errorDetails = new ErrorDetails(exception);

        // Check details
        errorDetails.ClassName.Should().Be("KristofferStrube.Blazor.WebIDL.Exceptions.WebIDLException");
        errorDetails.Message.Should().Be("Test");

        // Check parsed stack trace
        errorDetails.StackTrace!.Count.Should().Be(3);
        var traceDetails = errorDetails.StackTrace!.First();
        traceDetails.ColumnNumber.Should().Be(9);
        traceDetails.FileName.Should().Be("http://localhost:5010/myfunctions.js");
        traceDetails.LineNumber.Should().Be(7);
        traceDetails.MethodName.Should().Be("causeErrors");
    }
}