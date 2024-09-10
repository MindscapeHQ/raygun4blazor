using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Raygun.Blazor;
using Raygun.Blazor.Interfaces;
using Raygun.Blazor.Models;
using Raygun.Blazor.Offline;

namespace Raygun.Tests.Blazor
{
    [TestClass]
    public class RaygunOfflineStoreTests
    {
        RaygunLocalOfflineStore _raygunOfflineStore;
        TestSendStrategy _testSendStrategy;

        [TestInitialize]
        public void Setup()
        {
            // Set a temp directory for the offline store
            var tempDirName = "TestDirectory";
            IOptions<RaygunSettings> options = Options.Create(new RaygunSettings
            {
                DirectoryName = tempDirName,
            });

            // Clean any remaining crash reports stored in the temp directory
            var tempDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), tempDirName);
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, true);
            }

            // Custom send strategy to test sending
            _testSendStrategy = new TestSendStrategy();

            // Actual implemention of the local offline store
            _raygunOfflineStore = new RaygunLocalOfflineStore(_testSendStrategy, options);
        }

        [TestMethod]
        public async Task RaygunLocalOfflineStore_Save()
        {
            // Verify the save method returns true
            var request = new RaygunRequest();
            var result = await _raygunOfflineStore.Save(request, new System.Threading.CancellationToken());
            result.Should().BeTrue();
        }

        [TestMethod]
        public async Task RaygunLocalOfflineStore_SendAll()
        {
            bool called = false;
            var request = new RaygunRequest();

            // Create a send handler
            SendHandler sendHandler = (RaygunRequest messagePayload, System.Threading.CancellationToken cancellationToken) =>
            {
                called = true;
                // verify the request is the same
                messagePayload.Should().BeEquivalentTo(request);
                return Task.FromResult(true);
            };

            // Set the send callback
            _raygunOfflineStore.SetSendCallback(sendHandler);

            // Save a request
            var result = await _raygunOfflineStore.Save(request, new System.Threading.CancellationToken());
            result.Should().BeTrue();

            // Tell the send strategy to send all
            await _testSendStrategy.SendAll();

            // Should have called the send handler
            called.Should().BeTrue();
        }
    }
}
