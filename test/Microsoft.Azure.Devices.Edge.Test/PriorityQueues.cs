// Copyright (c) Microsoft. All rights reserved.
namespace Microsoft.Azure.Devices.Edge.Test
{
    using Microsoft.Azure.Devices.Edge.Test.Common;
    using Microsoft.Azure.Devices.Edge.Test.Common.Config;
    using Microsoft.Azure.Devices.Edge.Test.Helpers;
    using Microsoft.Azure.Devices.Edge.Util.Test.Common.NUnit;
    using Newtonsoft.Json.Linq;
    using NUnit.Framework;
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;

    [EndToEnd]
    public class PriorityQueues : SasManualProvisioningFixture
    {
        [Test]
        public async Task PriorityQueueModuleToModuleMessages()
        {
            CancellationToken token = this.TestToken;
            string trcImage = Context.Current.TestResultCoordinatorImage.Expect(() => new ArgumentException("testResultCoordinatorImage parameter is required for Priority Queues test"));
            string loadGenImage = Context.Current.LoadGenImage.Expect(() => new ArgumentException("loadGenImage parameter is required for TempFilter test"));
            string relayerImage = Context.Current.RelayerImage.Expect(() => new ArgumentException("relayerImage parameter is required for TempFilter test"));

            const string trcModuleName = "testResultCoordinator";
            const string loadGenModuleName = "loadGenModule";
            const string relayerModuleName = "relayerModule";
            const string trcUrl = "http://" + trcModuleName + ":5001";

            Action<EdgeConfigBuilder> addInitialConfig = new Action<EdgeConfigBuilder>(
                builder =>
                {
                    builder.AddModule(trcModuleName, trcImage)
                       .WithEnvironment(new[]
                       {
                           ("trackingId", Guid.NewGuid().ToString()),
                           ("eventHubConnectionString", "Endpoint=sb://dybronsoeventhub.servicebus.windows.net/;EntityPath=DybronsoTestEventHub;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=7ycWedM+hFb8+PKGO2ST7Z9Q7X7VlWB1BSczFAnOs38="),
                           ("IOT_HUB_CONNECTION_STRING", "HostName=dybronso-iot-hub.azure-devices.net;SharedAccessKeyName=iothubowner;SharedAccessKey=g8BRuiPbFRLEttMsncI6aHUw21Jjr+AEb/Yf4brYD7Y="),
                           ("logAnalyticsWorkspaceId", "Unnecessary value for e2e test"),
                           ("logAnalyticsSharedKey", "Unnecessary value for e2e test"),
                           ("logAnalyticsLogType", "Unnecessary"),
                           ("testStartDelay", "00:00:00"),
                           ("testDuration", "00:01:30"),
                           ("verificationDelay", "00:00:10"),
                           ("STORAGE_ACCOUNT_CONNECTION_STRING", "Unnecessary value for e2e test"),
                           ("NetworkControllerRunProfile", "Online"),
                           ("TEST_INFO", "key=value")
                       })
                       .WithSettings(new[] { ("createOptions", "{\"HostConfig\": {\"PortBindings\": {\"5001/tcp\": [{\"HostPort\": \"5001\"}]}}}") })

                       .WithDesiredProperties(new Dictionary<string, object>
                       {
                           ["reportMetadataList"] = new Dictionary<string, object>
                           {
                               ["reportMetadata1"] = new Dictionary<string, object>
                               {
                                   ["TestReportType"] = "CountingReport",
                                   ["TestOperationResultType"] = "Messages",
                                   ["ExpectedSource"] = "loadGenModule.send",
                                   ["ActualSource"] = "relayerModule.receive",
                                   ["TestDescription"] = "this field isn't used by TRC for E2E tests"
                               }
                           }
                       });
                    builder.AddModule(loadGenModuleName, loadGenImage)
                        .WithEnvironment(new[]
                        {
                            ("testResultCoordinatorUrl", trcUrl),
                            ("senderType", "PriorityMessageSender"),
                            ("trackingId", "e2eTestTrackingId"),
                            ("testDuration", "00:00:10")
                        });

                    builder.GetModule(ModuleName.EdgeHub)
                        .WithDesiredProperties(new Dictionary<string, object>
                        {
                            ["routes"] = new
                            {
                                LoadGenToRelayer1 = "FROM /messages/modules/" + loadGenModuleName + "/outputs/pri0 INTO BrokeredEndpoint('/modules/" + relayerModuleName + "/inputs/input1')",
                                LoadGenToRelayer2 = "FROM /messages/modules/" + loadGenModuleName + "/outputs/pri1 INTO BrokeredEndpoint('/modules/" + relayerModuleName + "/inputs/input1')",
                                LoadGenToRelayer3 = "FROM /messages/modules/" + loadGenModuleName + "/outputs/pri2 INTO BrokeredEndpoint('/modules/" + relayerModuleName + "/inputs/input1')",
                                LoadGenToRelayer4 = "FROM /messages/modules/" + loadGenModuleName + "/outputs/pri3 INTO BrokeredEndpoint('/modules/" + relayerModuleName + "/inputs/input1')",
                            }
                        });
                });

            EdgeDeployment deployment = await this.runtime.DeployConfigurationAsync(addInitialConfig, token);

            // Wait for loadGen to send some messages
            await Task.Delay(TimeSpan.FromSeconds(20));

            Action<EdgeConfigBuilder> addRelayerConfig = new Action<EdgeConfigBuilder>(
                builder =>
                {
                    builder.AddModule(relayerModuleName, relayerImage)
                        .WithEnvironment(new[] { ("receiveOnly", "true") });
            });

            deployment = await this.runtime.DeployConfigurationAsync(addInitialConfig + addRelayerConfig, token);
            await Task.Delay(TimeSpan.FromSeconds(20));

            HttpClient client = new HttpClient();
            HttpResponseMessage response = await client.GetAsync("http://localhost:5001/api/report");
            var jsonstring = await response.Content.ReadAsStringAsync();
            Console.WriteLine("got json string: \n" + jsonstring);
            var objects = JArray.Parse(jsonstring);
            var report = objects[0];
            bool isPassed = Boolean.Parse((string)report["isPassed"]);
            Assert.IsTrue(isPassed);

            // Next steps:
            // fill in trc env vars
            // fill in other env vars
            // good to go? - find images of these bad boys and run it
            // Ask damon - Where are these containers run when you run these locally?
        }
    }   
}
