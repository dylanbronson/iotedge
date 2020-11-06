using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Client.Transport.Mqtt;
using System;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;


namespace DeviceThatCallsDMOnTempSensor
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Started");
            string cxString = "HostName=dybronso-iot-hub.azure-devices.net;DeviceId=leafForModuleDMToLeafTest;SharedAccessKey=j/XPe4vSzaBMG2vv7qjPM8lOy66vQ+ACKXgfyV45MzA=;GatewayHostName=localhost";
            var t = new MqttTransportSettings(TransportType.Mqtt_Tcp_Only);
            t.RemoteCertificateValidationCallback = (object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) => true;
            var ts = new ITransportSettings[] { t };
            Console.WriteLine("trying to connect device client........");
            ModuleClient moduleClient = await ModuleClient.CreateFromEnvironmentAsync(ts);
            // await deviceClient.OpenAsync();
            await deviceClient.
            Console.WriteLine("Method handler set... waiting to die....");
            while (true)
            {
                Console.WriteLine("twiddling thumbs...");
                await Task.Delay(TimeSpan.FromSeconds(20));
            }
        }
    }
}
