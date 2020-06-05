// Copyright (c) Microsoft. All rights reserved.
namespace Microsoft.Azure.Devices.Edge.Hub.Core
{
    using System.Threading.Tasks;
    using Microsoft.Azure.Devices.Edge.Storage;
    using Microsoft.Azure.Devices.Edge.Util;

    public class DeviceCapabilityModelIdStore : IDeviceCapabilityModelIdStore
    {
        readonly IKeyValueStore<string, string> deviceCapabilityModelIdStore;

        public DeviceCapabilityModelIdStore(IKeyValueStore<string, string> deviceCapabilityModelIdEntityStore)
        {
            this.deviceCapabilityModelIdStore = Preconditions.CheckNotNull(deviceCapabilityModelIdEntityStore, nameof(deviceCapabilityModelIdEntityStore));
        }

        public Task SetDeviceCapabilityModelId(string id, string deviceCapabilityModelId)
        {
            Preconditions.CheckNonWhiteSpace(id, nameof(id));
            return !string.IsNullOrWhiteSpace(deviceCapabilityModelId) ? this.deviceCapabilityModelIdStore.Put(id, deviceCapabilityModelId) : Task.CompletedTask;
        }

        public async Task<string> GetDeviceCapabilityModelId(string id)
        {
            Preconditions.CheckNonWhiteSpace(id, nameof(id));
            Option<string> deviceCapabilityModel = await this.deviceCapabilityModelIdStore.Get(id);
            return deviceCapabilityModel.GetOrElse(string.Empty);
        }
    }
}
