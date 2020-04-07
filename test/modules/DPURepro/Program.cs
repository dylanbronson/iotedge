namespace DPURepro
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Azure.Devices.Client;
    using Microsoft.Azure.Devices.Edge.ModuleUtil;
    using Microsoft.Azure.Devices.Edge.Util;
    using Microsoft.Azure.Devices.Shared;
    using Microsoft.Extensions.Logging;

    class Program
    {
        static readonly ILogger Logger = ModuleUtil.CreateLogger(nameof(DPURepro));

        static async Task Main(string[] args)
        {
            (CancellationTokenSource cts, ManualResetEventSlim completed, Option<object> handler) = ShutdownHandler.Init(TimeSpan.FromSeconds(5), Logger);

            ModuleClient moduleClient = await ModuleUtil.CreateModuleClientAsync(
                        TransportType.Amqp,
                        ModuleUtil.DefaultTimeoutErrorDetectionStrategy,
                        ModuleUtil.DefaultTransientRetryStrategy,
                        Logger);
            await moduleClient.SetDesiredPropertyUpdateCallbackAsync(ReceivedUpdate, null);

            // Uncomment out below to enable ReportedPropertyUpdates
            // int counter = 0;
            // while (true)
            // {
            //     await moduleClient.UpdateReportedPropertiesAsync(new TwinCollection($"{{\"json\":\"{counter}\"}}"));
            //     counter++;
            //     Logger.LogInformation($"Updated reported prop: {counter}");
            //     await Task.Delay(TimeSpan.FromSeconds(10));
            // }

             await cts.Token.WhenCanceled();
             completed.Set();
             handler.ForEach(h => GC.KeepAlive(h));
             Logger.LogInformation("TwinTester exiting.");
        }

        static Task ReceivedUpdate(TwinCollection twinCollection, object _)
        {
            Logger.LogInformation($"Received DesiredPropertyUpdate callback: {twinCollection.ToString()}");
            return Task.CompletedTask;
        }
    }
}
