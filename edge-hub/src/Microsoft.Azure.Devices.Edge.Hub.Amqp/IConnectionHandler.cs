// Copyright (c) Microsoft. All rights reserved.
namespace Microsoft.Azure.Devices.Edge.Hub.Amqp
{
    using System.Threading.Tasks;
    using Microsoft.Azure.Devices.Edge.Hub.Amqp.LinkHandlers;
    using Microsoft.Azure.Devices.Edge.Hub.Core.Device;
    using Microsoft.Azure.Devices.Edge.Util;

    public interface IConnectionHandler
    {
        Task<IDeviceListener> GetDeviceListener(Option<string> deviceCapabilityModelId);

        Task RegisterLinkHandler(ILinkHandler linkHandler);

        Task RemoveLinkHandler(ILinkHandler linkHandler);
    }
}
