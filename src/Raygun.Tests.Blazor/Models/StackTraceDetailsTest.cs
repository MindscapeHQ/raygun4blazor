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
    /// <summary>
    /// On Blazor Server the JS stack arrives from the client through a [JSInvokable] method, so
    /// any client can send it. The original backtracking regex took several seconds on this 2 KB
    /// line and grew cubically with length; parsing must stay linear.
    /// </summary>
    [TestMethod]
    public void StackTraceDetails_JavaScriptFrame_HostileInputParsesInLinearTime()
    {
        var frame = string.Concat(Enumerable.Repeat("@:1", 700));

        var stopwatch = Stopwatch.StartNew();
        var details = new StackTraceDetails(frame);
        stopwatch.Stop();

        details.LineNumber.Should().Be(0);
        stopwatch.Elapsed.Should().BeLessThan(TimeSpan.FromSeconds(1));
    }
}
