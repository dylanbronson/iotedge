using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Client.Transport.Mqtt;
using System;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace C2DModule
{
    class Program
    {
        static async Task Main(string[] args)
        {
            string cxnString = "replaceMe";
            var t = new MqttTransportSettings(TransportType.Mqtt_Tcp_Only);
            t.RemoteCertificateValidationCallback = (object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) => true;
            var ts = new ITransportSettings[] { t };
            var deviceClient = DeviceClient.CreateFromConnectionString(cxnString, ts);
            Console.WriteLine("Waiting for message to be received...");
            while (true)
            {
                Message receivedMessage = await deviceClient.ReceiveAsync();
                if (receivedMessage == null) continue;

                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Received message: {0}",
                Encoding.ASCII.GetString(receivedMessage.GetBytes()));
                Console.ResetColor();

                await deviceClient.CompleteAsync(receivedMessage);
            }

        }
    }
}
