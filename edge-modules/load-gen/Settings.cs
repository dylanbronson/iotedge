// Copyright (c) Microsoft. All rights reserved.
namespace LoadGen
{
    using System;
    using System.IO;
    using Microsoft.Azure.Devices.Client;
    using Microsoft.Extensions.Configuration;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;
    using Newtonsoft.Json.Serialization;

    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public class Settings
    {
        static readonly Lazy<Settings> DefaultSettings = new Lazy<Settings>(
            () =>
            {
                IConfiguration configuration = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("config/settings.json", optional: true)
                    .AddEnvironmentVariables()
                    .Build();

                return new Settings(
                    configuration.GetValue("messageFrequency", TimeSpan.FromMilliseconds(20)),
                    configuration.GetValue("twinUpdateFrequency", TimeSpan.FromMilliseconds(500)),
                    configuration.GetValue<ulong>("messageSizeInBytes", 1024),
                    configuration.GetValue<TransportType>("transportType", TransportType.Amqp_Tcp_Only),
                    configuration.GetValue<string>("outputName", "output1"),
                    configuration.GetValue<int>("startDelay", 1));
            });

        Settings(
            TimeSpan messageFrequency,
            TimeSpan twinUpdateFrequency,
            ulong messageSizeInBytes,
            TransportType transportType,
            string outputName,
            int startDelay)
        {
            this.MessageFrequency = messageFrequency;
            this.TwinUpdateFrequency = twinUpdateFrequency;
            this.MessageSizeInBytes = messageSizeInBytes;
            this.TransportType = transportType;
            this.OutputName = outputName;
            this.StartDelay = startDelay;
        }

        public static Settings Current => DefaultSettings.Value;

        public TimeSpan MessageFrequency { get; }

        public TimeSpan TwinUpdateFrequency { get; }

        public ulong MessageSizeInBytes { get; }

        [JsonConverter(typeof(StringEnumConverter))]
        public TransportType TransportType { get; }

        public string OutputName { get; }

        public int StartDelay { get; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }
    }
}
