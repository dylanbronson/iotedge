// Copyright (c) Microsoft. All rights reserved.
namespace Microsoft.Azure.Devices.Edge.Hub.Core
{
    using System.Threading.Tasks;

    public interface IDeviceCapabilityModelIdStore
    {
        Task SetDeviceCapabilityModelId(string id, string deviceCapabilityModel);

        Task<string> GetDeviceCapabilityModelId(string id);
    }
}
