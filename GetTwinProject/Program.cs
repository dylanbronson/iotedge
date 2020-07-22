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
            Console.WriteLine($"Got twin: {twin.ToJson(Formatting.Indented)}");
        }
    }
}
