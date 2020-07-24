using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Shared;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace GetTwinProject
{
    class Program
    {
        static async Task Main(string[] args)
        {
            RegistryManager rm = RegistryManager.CreateFromConnectionString("HostName=dybronso-pnp-hub.azure-devices.net;SharedAccessKeyName=iothubowner;SharedAccessKey=3kFYaAqGG8grjL2nLU6EFMq9U0vDSAGKVFWpw2PZr9Q=");
            Twin twin = await rm.GetTwinAsync("dybronso-pnp-device", "PnPModule");
            Twin twin2 = await rm.GetTwinAsync("dybronso-pnp-device", "PnPModule2");
            Twin twin3 = await rm.GetTwinAsync("dybronso-pnp-device", "PnPModule3");
            Twin twin4 = await rm.GetTwinAsync("dybronso-pnp-device", "PnPModule4");
            Console.WriteLine($"Got twin1: {twin.ToJson(Formatting.Indented)}");
            Console.WriteLine($"Got twin2: {twin2.ToJson(Formatting.Indented)}");
            Console.WriteLine($"Got twin3: {twin3.ToJson(Formatting.Indented)}");
            Console.WriteLine($"Got twin4: {twin4.ToJson(Formatting.Indented)}");
        }
    }
}
