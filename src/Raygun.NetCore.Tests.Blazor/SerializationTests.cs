using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Raygun.NetCore.Blazor.Models;
using System;
using System.Text.Json;

namespace Raygun4Net.Tests.Blazor
{

    /// <summary>
    /// Tests whether the RaygunRequest object can be properly deserialized from a JSON payload.
    /// </summary>
    [TestClass]
    public class SerializationTests
    {

        #region Private Members

        private const string apiDocsPayload = "{\r\n  \"occurredOn\": \"2015-09-08T01:55:28Z\",\r\n  \"details\": {\r\n    \"machineName\": \"ServerMachine1\",\r\n    \"groupingKey\": \"ErrorGroup\",\r\n    \"version\": \"1.0.0.1\",\r\n    \"client\": {\r\n      \"name\": \"Example Raygun Client\",\r\n      \"version\": \"0.0.0.1\",\r\n      \"clientUrl\": \"/documentation/integrations/api\"\r\n    },\r\n    \"error\": {\r\n      \"innerError\": {},\r\n      \"data\": {\r\n        \"example\": 5\r\n      },\r\n      \"className\": \"ErrorClass\",\r\n      \"message\": \"An error occurred\",\r\n      \"stackTrace\": [\r\n        {\r\n          \"lineNumber\": 55,\r\n          \"className\": \"BrokenService\",\r\n          \"columnNumber\": 23,\r\n          \"fileName\": \"BrokenService.cs\",\r\n          \"methodName\": \"BreakEverything()\"\r\n        }\r\n      ]\r\n    },\r\n    \"environment\": {\r\n      \"processorCount\": 4,\r\n      \"osVersion\": \"Windows 10\",\r\n      \"windowBoundsWidth\": 2560,\r\n      \"windowBoundsHeight\": 1440,\r\n      \"browser-Width\": 2560,\r\n      \"browser-Height\": 1440,\r\n      \"screen-Width\": 2560,\r\n      \"screen-Height\": 1440,\r\n      \"resolutionScale\": 1,\r\n      \"color-Depth\": 24,\r\n      \"currentOrientation\": \"Landscape\",\r\n      \"cpu\": \"Intel(R) Core(TM) i5-2500 CPU @ 3.30GHz\",\r\n      \"packageVersion\": \"package version\",\r\n      \"architecture\": \"ARMv7-A\",\r\n      \"deviceManufacturer\": \"Nokia\",\r\n      \"model\": \"Lumia 920\",\r\n      \"totalPhysicalMemory\": 1024,\r\n      \"availablePhysicalMemory\": 500,\r\n      \"totalVirtualMemory\": 500,\r\n      \"availableVirtualMemory\": 500,\r\n      \"diskSpaceFree\": [\r\n        50000.52,\r\n        2000.104\r\n      ],\r\n      \"deviceName\": \"Nexus 7\",\r\n      \"locale\": \"en-nz\",\r\n      \"utcOffset\": -12,\r\n      \"browser\": \"Mozilla\",\r\n      \"browserName\": \"Netscape\",\r\n      \"browser-Version\": \"5.0 (Windows NT 6.3; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/39.0.2171.65 Safari/537.36\",\r\n      \"platform\": \"Win32\"\r\n    },\r\n    \"tags\": [\r\n      \"tag1\",\r\n      \"tag 2\",\r\n      \"tag-3\"\r\n    ],\r\n    \"userCustomData\": {\r\n      \"domain\": \"WORKPLACE\",\r\n      \"area\": \"51\"\r\n    },\r\n    \"request\": {\r\n      \"hostName\": \"https://raygun.io\",\r\n      \"url\": \"/documentation/integrations/api\",\r\n      \"httpMethod\": \"POST\",\r\n      \"iPAddress\": \"127.0.0.1\",\r\n      \"queryString\": {\r\n        \"q\": \"searchParams\"\r\n      },\r\n      \"form\": {\r\n        \"firstName\": \"Example\",\r\n        \"lastName\": \"Person\",\r\n        \"newsletter\": true\r\n      },\r\n      \"headers\": {\r\n        \"Referer\": \"www.google.com\",\r\n        \"Host\": \"raygun.io\"\r\n      },\r\n      \"rawData\": \"{\\\"Test\\\": 5}\"\r\n    },\r\n    \"response\": {\r\n      \"statusCode\": 500\r\n    },\r\n    \"user\": {\r\n      \"identifier\": \"123456789\",\r\n      \"isAnonymous\": false,\r\n      \"email\": \"test@example.com\",\r\n      \"fullName\": \"Test User\",\r\n      \"firstName\": \"Test\",\r\n      \"uuid\": \"783491e1-d4a9-46bc-9fde-9b1dd9ef6c6e\"\r\n    },\r\n    \"breadcrumbs\": [\r\n      {\r\n        \"timeStamp\": 1504799959639,\r\n        \"level\": \"info\",\r\n        \"type\": \"navigation\",\r\n        \"category\": \"checkout\",\r\n        \"message\": \"User navigated to the shopping cart\",\r\n        \"className\": \"ShoppingCart\",\r\n        \"methodName\": \"ViewBasket\",\r\n        \"lineNumber\": 156,\r\n        \"customData\": {\r\n          \"from\": \"/category/product/123\",\r\n          \"to\": \"/cart/view\"\r\n        }\r\n      }\r\n    ]\r\n  }\r\n}";
        private JsonSerializerOptions jsonSerializerOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, };

        #endregion

        #region Test Methods

        /// <summary>
        /// Tests if the RaygunRequest object can be properly deserialized from the official API reference payload.
        /// </summary>
        [TestMethod]
        public void ApiDocsPayload_ShouldDeserialize()
        {
            var result = JsonSerializer.Deserialize<RaygunRequest>(apiDocsPayload, jsonSerializerOptions);
            result.Should().NotBeNull();
            result.OccurredOn.Should().Be(new DateTime(2015, 09, 08, 01, 55, 28, DateTimeKind.Utc));
            result.Details.Should().NotBeNull();
            result.Details.MachineName.Should().Be("ServerMachine1");
            result.Details.GroupingKey.Should().Be("ErrorGroup");
            result.Details.ApplicationVersion.Should().Be("1.0.0.1");
            result.Details.Client.Should().NotBeNull();
            result.Details.Client.Name.Should().Be("Example Raygun Client");
            result.Details.Client.Version.Should().Be("0.0.0.1");
            result.Details.Client.ClientUrl.Should().Be("/documentation/integrations/api");
            result.Details.Error.Should().NotBeNull();
            result.Details.Error.InnerError.Should().NotBeNull();
            result.Details.Error.Data.Should().NotBeNull();
            //result.Details.Error.Data["example"].Should().Be(5);
            result.Details.Error.ClassName.Should().Be("ErrorClass");
            result.Details.Error.Message.Should().Be("An error occurred");
            result.Details.Error.StackTrace.Should().NotBeNull().And.HaveCount(1);
            result.Details.Error.StackTrace[0].LineNumber.Should().Be(55);
            result.Details.Error.StackTrace[0].ClassName.Should().Be("BrokenService");
            result.Details.Error.StackTrace[0].ColumnNumber.Should().Be(23);
            result.Details.Error.StackTrace[0].FileName.Should().Be("BrokenService.cs");
            result.Details.Error.StackTrace[0].MethodName.Should().Be("BreakEverything()");
            result.Details.Environment.Should().NotBeNull();
            result.Details.Environment.ProcessorCount.Should().Be(4);
            result.Details.Environment.OSVersion.Should().Be("Windows 10");
            result.Details.Environment.WindowBoundsWidth.Should().Be(2560);
            result.Details.Environment.WindowBoundsHeight.Should().Be(1440);
            result.Details.Environment.BrowserWidth.Should().Be(2560);
            result.Details.Environment.BrowserHeight.Should().Be(1440);
            result.Details.Environment.ScreenWidth.Should().Be(2560);
            result.Details.Environment.ScreenHeight.Should().Be(1440);
            result.Details.Environment.ResolutionScale.Should().Be(1);
            result.Details.Environment.ColorDepth.Should().Be(24);
            result.Details.Environment.CurrentOrientation.Should().Be("Landscape");
            result.Details.Environment.Cpu.Should().Be("Intel(R) Core(TM) i5-2500 CPU @ 3.30GHz");
            //result.Details.Environment.PackageVersion.Should().Be("package version");
            result.Details.Environment.Architecture.Should().Be("ARMv7-A");
            result.Details.Environment.DeviceManufacturer.Should().Be("Nokia");
            result.Details.Environment.DeviceModel.Should().Be("Lumia 920");
            result.Details.Environment.TotalPhysicalMemory.Should().Be(1024);
            result.Details.Environment.AvailablePhysicalMemory.Should().Be(500);
            result.Details.Environment.TotalVirtualMemory.Should().Be(500);
            result.Details.Environment.AvailableVirtualMemory.Should().Be(500);
            result.Details.Environment.DiskSpaceFree.Should().NotBeNull().And.HaveCount(2);
            result.Details.Environment.DiskSpaceFree[0].Should().Be(50000.52);
            result.Details.Environment.DiskSpaceFree[1].Should().Be(2000.104);
            result.Details.Environment.DeviceName.Should().Be("Nexus 7");
            result.Details.Environment.Locale.Should().Be("en-nz");
            result.Details.Environment.UtcOffset.Should().Be(-12);
            result.Details.Environment.BrowserManufacturer.Should().Be("Mozilla");
            result.Details.Environment.BrowserName.Should().Be("Netscape");
            result.Details.Environment.BrowserVersion.Should().Be("5.0 (Windows NT 6.3; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/39.0.2171.65 Safari/537.36");
            result.Details.Environment.Platform.Should().Be("Win32");
            result.Details.Tags.Should().NotBeNull().And.HaveCount(3);
            result.Details.Tags[0].Should().Be("tag1");
            result.Details.Tags[1].Should().Be("tag 2");
            result.Details.Tags[2].Should().Be("tag-3");
            result.Details.UserCustomData.Should().NotBeNull().And.HaveCount(2);
            result.Details.UserCustomData["domain"].Should().Be("WORKPLACE");
            result.Details.UserCustomData["area"].Should().Be("51");
            result.Details.Request.Should().NotBeNull();
            result.Details.Request.HostName.Should().Be("https://raygun.io");
            result.Details.Request.Url.Should().Be("/documentation/integrations/api");
            result.Details.Request.HttpMethod.Should().Be("POST");
            result.Details.Request.IPAddress.Should().Be("127.0.0.1");
            result.Details.Request.QueryString.Should().NotBeNull().And.HaveCount(1);
            result.Details.Request.QueryString["q"].Should().Be("searchParams");
            result.Details.Request.Form.Should().NotBeNull().And.HaveCount(3);
            result.Details.Request.Form["firstName"].ToString().Should().Be("Example");
            result.Details.Request.Form["lastName"].ToString().Should().Be("Person");
            result.Details.Request.Form["newsletter"].ToString().Should().Be("True");
            result.Details.Request.Headers.Should().NotBeNull().And.HaveCount(2);
            result.Details.Request.Headers["Referer"].Should().Be("www.google.com");
            result.Details.Request.Headers["Host"].Should().Be("raygun.io");
            result.Details.Request.RawData.Should().NotBeNullOrWhiteSpace();
            //result.Details.Response.Should().NotBeNull();
            //result.Details.Response.StatusCode.Should().Be(500);
            result.Details.User.Should().NotBeNull();
            result.Details.User.UserId.Should().Be("123456789");
            result.Details.User.IsAnonymous.Should().BeFalse();
            result.Details.User.Email.Should().Be("test@example.com");
            result.Details.User.FullName.Should().Be("Test User");
            result.Details.User.FirstName.Should().Be("Test");
            result.Details.User.DeviceId.Should().Be("783491e1-d4a9-46bc-9fde-9b1dd9ef6c6e");
            result.Details.Breadcrumbs.Should().NotBeNull().And.HaveCount(1);
            result.Details.Breadcrumbs[0].Timestamp.Should().Be(1504799959639);
            //result.Details.Breadcrumbs[0].Level.Should().Be("info");
            result.Details.Breadcrumbs[0].Type.Should().Be(BreadcrumbType.Navigation);
            result.Details.Breadcrumbs[0].Category.Should().Be("checkout");
            result.Details.Breadcrumbs[0].Message.Should().Be("User navigated to the shopping cart");
            result.Details.Breadcrumbs[0].ClassName.Should().Be("ShoppingCart");
            result.Details.Breadcrumbs[0].MethodName.Should().Be("ViewBasket");
            result.Details.Breadcrumbs[0].LineNumber.Should().Be(156);
            result.Details.Breadcrumbs[0].CustomData.Should().NotBeNull().And.HaveCount(2);
            result.Details.Breadcrumbs[0].CustomData["from"].ToString().Should().Be("/category/product/123");
            result.Details.Breadcrumbs[0].CustomData["to"].ToString().Should().Be("/cart/view");
        }

        #endregion

    }

}