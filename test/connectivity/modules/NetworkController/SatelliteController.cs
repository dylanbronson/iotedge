// Copyright (c) Microsoft. All rights reserved.
namespace NetworkController
{
    using System.Threading;
    using System.Threading.Tasks;
    using ModuleUtil.networkcontrollerreuslt;

    // TODO: implement satellite
    class SatelliteController : INetworkController
    {
        string name;

        public SatelliteController(string name)
        {
            this.name = name;
        }

        public NetworkStatus NetworkStatus => NetworkStatus.Sattelite;

        public Task<bool> SetEnabledAsync(bool enabled, CancellationToken cs)
        {
            return Task.FromResult(true);
        }

        public Task<bool> GetEnabledAsync(CancellationToken cs)
        {
            return Task.FromResult(true);
        }
    }
}
