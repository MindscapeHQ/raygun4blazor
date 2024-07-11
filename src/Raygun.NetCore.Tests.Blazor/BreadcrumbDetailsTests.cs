using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Raygun.NetCore.Blazor.Models;
using System.Text.Json;

namespace Raygun.NetCore.Tests.Blazor
{

    /// <summary>
    /// Tests the functionality of the <see cref="BreadcrumbDetails"/> class.
    /// </summary>
    [TestClass]
    public class BreadcrumbDetailsTests
    {

        #region Test Methods

        /// <summary>
        /// Tests whether a new instance of <see cref="BreadcrumbDetails"/> can be created using the private serializer constructor.
        /// </summary>
        [TestMethod]
        public void Breadcrumbs_NewInstance_Serialization()
        {
            var bc = JsonSerializer.Deserialize<BreadcrumbDetails>("{}");
            bc.Should().NotBeNull();
            bc.Timestamp.Should().Be(0);
            bc.Message.Should().BeNullOrWhiteSpace();
            bc.MethodName.Should().BeNullOrWhiteSpace();
            bc.LineNumber.Should().Be(0);
        }

        /// <summary>
        /// Tests whether a new instance of <see cref="BreadcrumbDetails"/> can be created using the default constructor.
        /// </summary>
        [TestMethod]
        public void Breadcrumbs_NewInstance_MessageOverload()
        {
            var bc = new BreadcrumbDetails("Test");
            bc.Should().NotBeNull();
            bc.Timestamp.Should().BePositive();
            bc.Message.Should().Be("Test");
            bc.MethodName.Should().Be(nameof(Breadcrumbs_NewInstance_MessageOverload));
            bc.LineNumber.Should().Be(25);
        }

        #endregion

    }

}
