using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Client.Transport.Mqtt;
using System;
using System.Threading.Tasks;

namespace ModuleToCallDM
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Opening now!");
            var ts = new ITransportSettings[] { new MqttTransportSettings(TransportType.Mqtt_Tcp_Only) };
            ModuleClient moduleClient = ModuleClient.CreateFromConnectionString("HostName=dybronso-iot-hub.azure-devices.net;DeviceId=dybronso1_L4_edge;ModuleId=ModuleToCallDm;SharedAccessKey=7chVGVaJ18heggSADh3RE49xdkpAr1tHp6FcbzEudNM=;GatewayHostName=dybronso1l4edge.eastus2.cloudapp.azure.com", ts);
            await moduleClient.OpenAsync();
            Console.WriteLine("Opened up module. Now waiting 20");
            await Task.Delay(TimeSpan.FromSeconds(20));
            MethodRequest mr = new MethodRequest("WriteToConsole");
            while (true)
            {
                Console.WriteLine("Invoking method...");
                await moduleClient.InvokeMethodAsync("leafForModuleDMToLeafTest", mr);
                Console.WriteLine("Method invoked. waiting 10");
                await Task.Delay(TimeSpan.FromSeconds(10));
            }
        }
    }
}
