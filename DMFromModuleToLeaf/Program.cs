using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Client.Transport.Mqtt;
using System;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace DMFromModuleToLeaf
{
    // This program will be a module sending a DM to a leaf device
    class Program
    {
        static async Task Main()
        {
            Console.WriteLine("Started");
            string cxString = "HostName=dybronso-iot-hub.azure-devices.net;DeviceId=leafForModuleDMToLeafTest;SharedAccessKey=j/XPe4vSzaBMG2vv7qjPM8lOy66vQ+ACKXgfyV45MzA=;GatewayHostName=localhost";
            var t = new MqttTransportSettings(TransportType.Mqtt_Tcp_Only);
            t.RemoteCertificateValidationCallback = (object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) => true;
            var ts = new ITransportSettings[] { t };
            Console.WriteLine("trying to connect device client........");
            DeviceClient deviceClient = DeviceClient.CreateFromConnectionString(cxString, ts);
            // await deviceClient.OpenAsync();
            await deviceClient.SetMethodHandlerAsync("WriteToConsole", WriteToConsoleAsync, null);
            Console.WriteLine("Method handler set... waiting to die....");
            while (true)
            {
                Console.WriteLine("twiddling thumbs...");
                await Task.Delay(TimeSpan.FromSeconds(20));
            }
        }

        private static Task<MethodResponse> WriteToConsoleAsync(MethodRequest methodRequest, object userContext)
        {
            Console.WriteLine($"\t *** {methodRequest.Name} was called.");
            Console.WriteLine($"\t{methodRequest.DataAsJson}\n");

            return Task.FromResult(new MethodResponse(new byte[0], 200));
        }

    }
}
