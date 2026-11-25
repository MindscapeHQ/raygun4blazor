using System;
using System.Diagnostics;
using System.Linq;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Raygun.Blazor.Models;

namespace Raygun.Tests.Blazor.Models;

[TestClass]
public class StackTraceDetailsTest
{
    [TestMethod]
    // Chrome and Edge (V8)
    [DataRow("at http://localhost:5010/smoke.js:24:64", "?", "http://localhost:5010/smoke.js", 24, 64)]
    [DataRow("at async http://localhost:5010/app.js:5:28", "?", "http://localhost:5010/app.js", 5, 28)]
    [DataRow("at causeErrors (http://localhost:5010/myfunctions.js:7:9)", "causeErrors", "http://localhost:5010/myfunctions.js", 7, 9)]
    [DataRow("at fn (https://unpkg.com/react@18.2.0/umd/react.production.min.js:1:2)", "fn", "https://unpkg.com/react@18.2.0/umd/react.production.min.js", 1, 2)]
    [DataRow("at new Foo (http://localhost:5010/app.js:1:2)", "new Foo", "http://localhost:5010/app.js", 1, 2)]
    [DataRow("at HTMLButtonElement.<anonymous> (https://example.com/_framework/blazor.web.js:1:2345)", "HTMLButtonElement.<anonymous>", "https://example.com/_framework/blazor.web.js", 1, 2345)]
    [DataRow("at async load (http://localhost:5010/app.js:3:4)", "async load", "http://localhost:5010/app.js", 3, 4)]
    [DataRow("at eval (eval at <anonymous> (http://localhost:5010/app.js:53:40), <anonymous>:3:7)", "eval", "eval at <anonymous> (http://localhost:5010/app.js:53:40), <anonymous>", 3, 7)]
    [DataRow("at Array.forEach (<anonymous>)", "Array.forEach", "<anonymous>", 0, 0)]
    [DataRow("at async Promise.all (index 0)", "async Promise.all", "index 0", 0, 0)]
    [DataRow("at http://localhost:5010/_framework/dotnet.native.wasm:wasm-function[313]:0x1d6b8", "?", "http://localhost:5010/_framework/dotnet.native.wasm:wasm-function[313]:0x1d6b8", 0, 0)]
    // Firefox (SpiderMonkey) and Safari (JavaScriptCore)
    [DataRow("causeErrors@http://localhost:5010/myfunctions.js:7:9", "causeErrors", "http://localhost:5010/myfunctions.js", 7, 9)]
    [DataRow("@http://localhost:5010/smoke.js:24:77", "?", "http://localhost:5010/smoke.js", 24, 77)]
    [DataRow("fn@https://unpkg.com/react@18.2.0/umd/react.production.min.js:1:2", "fn", "https://unpkg.com/react@18.2.0/umd/react.production.min.js", 1, 2)]
    [DataRow("@http://localhost/node_modules/@angular/core/core.js:3:4", "?", "http://localhost/node_modules/@angular/core/core.js", 3, 4)]
    [DataRow("./node_modules/@scope/pkg/index.js@http://localhost:5010/app.js:23:135", "./node_modules/@scope/pkg/index.js", "http://localhost:5010/app.js", 23, 135)]
    [DataRow("@click@http://localhost:5010/app.js:25:86", "@click", "http://localhost:5010/app.js", 25, 86)]
    [DataRow("EventHandlerNonNull*@http://localhost:5010/:18:9", "EventHandlerNonNull*", "http://localhost:5010/", 18, 9)]
    [DataRow("async*foo@http://localhost:5010/app.js:1:2", "async*foo", "http://localhost:5010/app.js", 1, 2)]
    [DataRow("foo/<@http://localhost:5010/app.js:1:2", "foo/<", "http://localhost:5010/app.js", 1, 2)]
    [DataRow("global code@http://localhost:5010/app.js:1:2", "global code", "http://localhost:5010/app.js", 1, 2)]
    [DataRow("forEach@[native code]", "forEach", "[native code]", 0, 0)]
    [DataRow("anonymous@", "anonymous", null, 0, 0)]
    // Not a frame: kept whole
    [DataRow("[native code]", null, "[native code]", 0, 0)]
    // Line number too large for an int: kept as an unparsed location
    [DataRow("fn@x:99999999999:1", "fn", "x:99999999999:1", 0, 0)]
    public void StackTraceDetails_JavaScriptFrame_ParsesFields(string frame, string methodName, string fileName, int lineNumber, int columnNumber)
    {
        var details = new StackTraceDetails(frame);

        details.MethodName.Should().Be(methodName);
        details.FileName.Should().Be(fileName);
        details.LineNumber.Should().Be(lineNumber);
        details.ColumnNumber.Should().Be(columnNumber);
    }

    /// <summary>
    /// On Blazor Server the JS stack arrives from the client through a [JSInvokable] method, so
    /// any client can send it. The original backtracking regex took several seconds on this 2 KB
    /// line and grew cubically with length; parsing must stay linear.
    /// </summary>
    [TestMethod]
    // The original regex on 2 KB.
    [DataRow("", "@:1", 700, "")]
    // The Chrome/Edge pattern on 64 KB, which takes several seconds without NonBacktracking.
    [DataRow("at a (", " ()", 21333, " (x")]
    public void StackTraceDetails_JavaScriptFrame_HostileInputParsesInLinearTime(string prefix, string fragment, int count, string suffix)
    {
        var frame = prefix + string.Concat(Enumerable.Repeat(fragment, count)) + suffix;

        var stopwatch = Stopwatch.StartNew();
        var details = new StackTraceDetails(frame);
        stopwatch.Stop();

        details.LineNumber.Should().Be(0);
        stopwatch.Elapsed.Should().BeLessThan(TimeSpan.FromSeconds(1));
    }
}
