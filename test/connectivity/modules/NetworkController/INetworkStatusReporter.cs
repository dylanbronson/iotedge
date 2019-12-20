// Copyright (c) Microsoft. All rights reserved.
namespace NetworkController
{
    using System.Threading.Tasks;
    using ModuleUtil.networkcontrollerreuslt;

    interface INetworkStatusReporter
    {
        Task ReportNetworkStatus(NetworkControllerOperation settingRule, bool enabled, NetworkStatus networkStatus, bool success = true);
    }
}
