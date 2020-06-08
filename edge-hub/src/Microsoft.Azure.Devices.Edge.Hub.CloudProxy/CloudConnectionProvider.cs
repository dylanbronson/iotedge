// Copyright (c) Microsoft. All rights reserved.
namespace Microsoft.Azure.Devices.Edge.Hub.CloudProxy
{
    using System;
    using System.ComponentModel;
    using System.Net;
    using System.Reflection;
    using System.Threading.Tasks;
    using Microsoft.Azure.Devices.Client;
    using Microsoft.Azure.Devices.Client.Transport.Mqtt;
    using Microsoft.Azure.Devices.Edge.Hub.Core;
    using Microsoft.Azure.Devices.Edge.Hub.Core.Cloud;
    using Microsoft.Azure.Devices.Edge.Hub.Core.Identity;
    using Microsoft.Azure.Devices.Edge.Hub.Core.Identity.Service;
    using Microsoft.Azure.Devices.Edge.Util;
    using Microsoft.Extensions.Logging;

    public class CloudConnectionProvider : ICloudConnectionProvider
    {
        static readonly TimeSpan HeartbeatTimeout = TimeSpan.FromSeconds(60);

        readonly Option<UpstreamProtocol> upstreamProtocol;
        readonly int connectionPoolSize;
        readonly Option<IWebProxy> proxy;
        readonly IMessageConverterProvider messageConverterProvider;
        readonly IClientProvider clientProvider;
        readonly TimeSpan idleTimeout;
        readonly ITokenProvider edgeHubTokenProvider;
        readonly IDeviceScopeIdentitiesCache deviceScopeIdentitiesCache;
        readonly bool closeOnIdleTimeout;
        readonly bool useServerHeartbeat;
        readonly ICredentialsCache credentialsCache;
        readonly IIdentity edgeHubIdentity;
        readonly TimeSpan operationTimeout;
        readonly IProductInfoStore productInfoStore;
        Option<IEdgeHub> edgeHub;

        public CloudConnectionProvider(
            IMessageConverterProvider messageConverterProvider,
            int connectionPoolSize,
            IClientProvider clientProvider,
            Option<UpstreamProtocol> upstreamProtocol,
            ITokenProvider edgeHubTokenProvider,
            IDeviceScopeIdentitiesCache deviceScopeIdentitiesCache,
            ICredentialsCache credentialsCache,
            IIdentity edgeHubIdentity,
            TimeSpan idleTimeout,
            bool closeOnIdleTimeout,
            TimeSpan operationTimeout,
            bool useServerHeartbeat,
            Option<IWebProxy> proxy,
            IProductInfoStore productInfoStore)
        {
            this.messageConverterProvider = Preconditions.CheckNotNull(messageConverterProvider, nameof(messageConverterProvider));
            this.clientProvider = Preconditions.CheckNotNull(clientProvider, nameof(clientProvider));
            this.upstreamProtocol = upstreamProtocol;
            this.connectionPoolSize = Preconditions.CheckRange(connectionPoolSize, 1, nameof(connectionPoolSize));
            this.proxy = proxy;
            this.edgeHub = Option.None<IEdgeHub>();
            this.idleTimeout = idleTimeout;
            this.closeOnIdleTimeout = closeOnIdleTimeout;
            this.useServerHeartbeat = useServerHeartbeat;
            this.edgeHubTokenProvider = Preconditions.CheckNotNull(edgeHubTokenProvider, nameof(edgeHubTokenProvider));
            this.deviceScopeIdentitiesCache = Preconditions.CheckNotNull(deviceScopeIdentitiesCache, nameof(deviceScopeIdentitiesCache));
            this.credentialsCache = Preconditions.CheckNotNull(credentialsCache, nameof(credentialsCache));
            this.edgeHubIdentity = Preconditions.CheckNotNull(edgeHubIdentity, nameof(edgeHubIdentity));
            this.operationTimeout = operationTimeout;
            this.productInfoStore = Preconditions.CheckNotNull(productInfoStore, nameof(productInfoStore));
        }

        public void BindEdgeHub(IEdgeHub edgeHubInstance)
        {
            this.edgeHub = Option.Some(Preconditions.CheckNotNull(edgeHubInstance, nameof(edgeHubInstance)));
        }

        public async Task<Try<ICloudConnection>> Connect(IClientCredentials clientCredentials, Action<string, CloudConnectionStatus> connectionStatusChangedHandler)
        {
            Preconditions.CheckNotNull(clientCredentials, nameof(clientCredentials));

            try
            {
                Events.CreatingCloudConnectionUsingClientCredentials(clientCredentials);
                var cloudListener = new CloudListener(this.edgeHub.Expect(() => new InvalidOperationException("EdgeHub reference should not be null")), clientCredentials.Identity.Id);
                string productInfo = await this.productInfoStore.GetEdgeProductInfo(clientCredentials.Identity.Id);
                ITransportSettings[] transportSettings = GetTransportSettings(
                    this.upstreamProtocol,
                    this.connectionPoolSize,
                    this.proxy,
                    this.useServerHeartbeat,
                    Option.None<string>());

                if (this.edgeHubIdentity.Id.Equals(clientCredentials.Identity.Id))
                {
                    ICloudConnection cc = await CloudConnection.Create(
                        clientCredentials.Identity,
                        connectionStatusChangedHandler,
                        transportSettings,
                        this.messageConverterProvider,
                        this.clientProvider,
                        cloudListener,
                        this.edgeHubTokenProvider,
                        this.idleTimeout,
                        this.closeOnIdleTimeout,
                        this.operationTimeout,
                        productInfo);
                    Events.SuccessCreatingCloudConnection(clientCredentials.Identity);
                    return Try.Success(cc);
                }
                else if (clientCredentials is ITokenCredentials clientTokenCredentails)
                {
                    ICloudConnection cc = await ClientTokenCloudConnection.Create(
                        clientTokenCredentails,
                        connectionStatusChangedHandler,
                        transportSettings,
                        this.messageConverterProvider,
                        this.clientProvider,
                        cloudListener,
                        this.idleTimeout,
                        this.closeOnIdleTimeout,
                        this.operationTimeout,
                        productInfo);
                    Events.SuccessCreatingCloudConnection(clientCredentials.Identity);
                    return Try.Success(cc);
                }
                else
                {
                    throw new InvalidOperationException($"Cannot connect using client credentials of type {clientCredentials.AuthenticationType} for identity {clientCredentials.Identity.Id}");
                }
            }
            catch (Exception ex)
            {
                Events.ErrorCreatingCloudConnection(clientCredentials.Identity, ex);
                return Try<ICloudConnection>.Failure(ex);
            }
        }

        public async Task<Try<ICloudConnection>> Connect(IIdentity identity, Option<string> deviceCapabilityModelId, Action<string, CloudConnectionStatus> connectionStatusChangedHandler)
        {
            Preconditions.CheckNotNull(identity, nameof(identity));

            try
            {
                var cloudListener = new CloudListener(this.edgeHub.Expect(() => new InvalidOperationException("EdgeHub reference should not be null")), identity.Id);
                Option<ServiceIdentity> serviceIdentity = (await this.deviceScopeIdentitiesCache.GetServiceIdentity(identity.Id))
                    .Filter(s => s.Status == ServiceIdentityStatus.Enabled);
                ITransportSettings[] transportSettings = GetTransportSettings(
                    this.upstreamProtocol,
                    this.connectionPoolSize,
                    this.proxy,
                    this.useServerHeartbeat,
                    deviceCapabilityModelId);
                return await serviceIdentity
                    .Map(
                        async si =>
                        {
                            Events.CreatingCloudConnectionOnBehalfOf(identity);
                            string productInfo = await this.productInfoStore.GetEdgeProductInfo(identity.Id);
                            ICloudConnection cc = await CloudConnection.Create(
                                identity,
                                connectionStatusChangedHandler,
                                transportSettings,
                                this.messageConverterProvider,
                                this.clientProvider,
                                cloudListener,
                                this.edgeHubTokenProvider,
                                this.idleTimeout,
                                this.closeOnIdleTimeout,
                                this.operationTimeout,
                                productInfo);
                            Events.SuccessCreatingCloudConnection(identity);
                            return Try.Success(cc);
                        })
                    .GetOrElse(
                        async () =>
                        {
                            Events.ServiceIdentityNotFound(identity);
                            Option<IClientCredentials> clientCredentials = await this.credentialsCache.Get(identity);
                            return await clientCredentials
                                .Map(cc => this.Connect(cc, connectionStatusChangedHandler))
                                .GetOrElse(() => throw new InvalidOperationException($"Unable to find identity {identity.Id} in device scopes cache or credentials cache"));
                        });
            }
            catch (Exception ex)
            {
                Events.ErrorCreatingCloudConnection(identity, ex);
                return Try<ICloudConnection>.Failure(ex);
            }
        }

        static ITransportSettings[] GetAmqpTransportSettings(TransportType type, int connectionPoolSize, Option<IWebProxy> proxy, bool useServerHeartbeat, Option<string> deviceCapabilityModelId)
        {
            var settings = new AmqpTransportSettings(type)
            {
                AmqpConnectionPoolSettings = new AmqpConnectionPoolSettings
                {
                    Pooling = true,
                    MaxPoolSize = (uint)connectionPoolSize
                }
            };

            if (useServerHeartbeat)
            {
                settings.IdleTimeout = HeartbeatTimeout;
            }

            proxy.ForEach(p => settings.Proxy = p);

            deviceCapabilityModelId.ForEach(dcmid =>
                settings.GetType()
                    .GetProperty("DeviceCapabilityModelId", BindingFlags.NonPublic | BindingFlags.Instance)
                    .SetValue(settings, dcmid));

            return new ITransportSettings[] { settings };
        }

        static ITransportSettings[] GetMqttTransportSettings(TransportType type, Option<IWebProxy> proxy, Option<string> deviceCapabilityModelId)
        {
            var settings = new MqttTransportSettings(type);
            proxy.ForEach(p => settings.Proxy = p);
            deviceCapabilityModelId.ForEach(dcmid =>
                settings.GetType()
                    .GetProperty("DeviceCapabilityModelId", BindingFlags.NonPublic | BindingFlags.Instance)
                    .SetValue(settings, dcmid));
            return new ITransportSettings[] { settings };
        }

        internal static ITransportSettings[] GetTransportSettings(Option<UpstreamProtocol> upstreamProtocol, int connectionPoolSize, Option<IWebProxy> proxy, bool useServerHeartbeat, Option<string> deviceCapabilityModelid)
        {
            return upstreamProtocol
                .Map(
                    up =>
                    {
                        switch (up)
                        {
                            case UpstreamProtocol.Amqp:
                                return GetAmqpTransportSettings(TransportType.Amqp_Tcp_Only, connectionPoolSize, proxy, useServerHeartbeat, deviceCapabilityModelid);

                            case UpstreamProtocol.AmqpWs:
                                return GetAmqpTransportSettings(TransportType.Amqp_WebSocket_Only, connectionPoolSize, proxy, useServerHeartbeat, deviceCapabilityModelid);

                            case UpstreamProtocol.Mqtt:
                                return GetMqttTransportSettings(TransportType.Mqtt_Tcp_Only, proxy, deviceCapabilityModelid);

                            case UpstreamProtocol.MqttWs:
                                return GetMqttTransportSettings(TransportType.Mqtt_WebSocket_Only, proxy, deviceCapabilityModelid);

                            default:
                                throw new InvalidEnumArgumentException($"Unsupported transport type {up}");
                        }
                    })
                .GetOrElse(
                    () => GetAmqpTransportSettings(TransportType.Amqp_Tcp_Only, connectionPoolSize, proxy, useServerHeartbeat, deviceCapabilityModelid));
        }

        static class Events
        {
            const int IdStart = CloudProxyEventIds.CloudConnectionProvider;
            static readonly ILogger Log = Logger.Factory.CreateLogger<CloudConnection>();

            enum EventIds
            {
                CloudConnectError = IdStart,
                CloudConnectSuccess,
                CreatingCloudConnectionUsingClientCredentials,
                CreatingCloudConnectionOnBehalfOf,
                ServiceIdentityNotFound
            }

            public static void SuccessCreatingCloudConnection(IIdentity identity)
            {
                Log.LogInformation((int)EventIds.CloudConnectSuccess, $"Created cloud connection for client {identity.Id}");
            }

            public static void ErrorCreatingCloudConnection(IIdentity identity, Exception exception)
            {
                Log.LogWarning((int)EventIds.CloudConnectError, exception, $"Error creating cloud connection for client {identity.Id}");
            }

            public static void CreatingCloudConnectionUsingClientCredentials(IClientCredentials clientCredentials)
            {
                Log.LogDebug((int)EventIds.CreatingCloudConnectionUsingClientCredentials, $"Creating cloud connection for client {clientCredentials.Identity.Id} using client credentials");
            }

            public static void CreatingCloudConnectionOnBehalfOf(IIdentity identity)
            {
                Log.LogDebug((int)EventIds.CreatingCloudConnectionOnBehalfOf, $"Creating cloud connection for client {identity.Id} using EdgeHub credentials");
            }

            public static void ServiceIdentityNotFound(IIdentity identity)
            {
                Log.LogDebug((int)EventIds.ServiceIdentityNotFound, $"Creating cloud connection for client {identity.Id}. Client identity is not in device scope, attempting to use client credentials.");
            }
        }
    }
}
