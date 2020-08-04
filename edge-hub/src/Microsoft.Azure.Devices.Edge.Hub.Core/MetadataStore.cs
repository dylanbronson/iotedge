// Copyright (c) Microsoft. All rights reserved.
namespace Microsoft.Azure.Devices.Edge.Hub.Core
{
    using System.Threading.Tasks;
    using Microsoft.Azure.Devices.Edge.Storage;
    using Microsoft.Azure.Devices.Edge.Util;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;

    public class MetadataStore : IMetadataStore
    {
        readonly IKeyValueStore<string, string> metadataEntityStore;
        readonly string edgeProductInfo;
        static readonly ILogger Log = Logger.Factory.CreateLogger<MetadataStore>();

        public MetadataStore(IKeyValueStore<string, string> metadataEntityStore, string edgeProductInfo)
        {
            this.metadataEntityStore = Preconditions.CheckNotNull(metadataEntityStore);
            this.edgeProductInfo = Preconditions.CheckNotNull(edgeProductInfo, nameof(edgeProductInfo));
        }

        public async Task<ConnectionMetadata> GetMetadata(string id)
        {
            Log.LogInformation("DRB - getting metadata");
            Option<string> value = await this.metadataEntityStore.Get(id);
            return await value.Match(
                async v =>
                {
                    Log.LogInformation($"DRB - got value from metadataStore {v}.");
                    return await this.GetOrMigrateConnectionMetadata(id, v);
                },
                () =>
                {
                    Log.LogInformation($"DRB - new metadata.");
                    return Task.FromResult(new ConnectionMetadata(this.edgeProductInfo));
                });
        }

        async Task<ConnectionMetadata> GetOrMigrateConnectionMetadata(string id, string entityValue)
        {
            if (this.TryDeserialize(entityValue, out ConnectionMetadata metadata))
            {
                Log.LogInformation($"DRB - found value {entityValue}. No need to migrate.");
                return metadata;
            }
            else
            {
                // Perform the migration by setting the new metadata object instead of the old productInfo string
                Log.LogInformation($"DRB - caught exception. Old value: {entityValue}. Migrating...");
                await this.SetMetadata(id, metadata);
                return metadata;
            }
        }

        bool TryDeserialize(string entityValue, out ConnectionMetadata metadata)
        {
            try
            {
                metadata = JsonConvert.DeserializeObject<ConnectionMetadata>(entityValue);
                return true;
            }
            catch (JsonException)
            {
                // If deserialization fails, assume the string is an old productInfo.
                // We must do this only for migration purposes, since this store used to just be a productInfoStore.
                metadata = new ConnectionMetadata(entityValue, this.edgeProductInfo);
                return false;
            }
        }

        async Task SetMetadata(string id, ConnectionMetadata metadata) => await this.metadataEntityStore.Put(id, JsonConvert.SerializeObject(metadata));

        public async Task SetMetadata(string id, string productInfo, Option<string> modelId)
        {
            ConnectionMetadata metadata = new ConnectionMetadata(productInfo, modelId, this.edgeProductInfo);
            await this.metadataEntityStore.Put(id, JsonConvert.SerializeObject(metadata));
        }

        public async Task SetModelId(string id, string modelId)
        {
            Log.LogInformation($"DRB - setting modelId: {modelId} for id {id}");
            Preconditions.CheckNonWhiteSpace(id, nameof(id));
            if (!string.IsNullOrWhiteSpace(modelId))
            {
                ConnectionMetadata oldOrEmptyMetadata = await this.GetMetadata(id);
                ConnectionMetadata newMetadata = new ConnectionMetadata(oldOrEmptyMetadata.ProductInfo, Option.Some(modelId), this.edgeProductInfo);
                await this.SetMetadata(id, newMetadata);
            }
        }

        public async Task SetProductInfo(string id, string productInfo)
        {
            Log.LogInformation($"DRB - setting product info: {productInfo} for id {id}");
            Preconditions.CheckNonWhiteSpace(id, nameof(id));
            ConnectionMetadata oldOrEmptyMetadata = await this.GetMetadata(id);
            ConnectionMetadata newMetadata = new ConnectionMetadata(productInfo, oldOrEmptyMetadata.ModelId, this.edgeProductInfo);
            await this.SetMetadata(id, newMetadata);
        }
    }
}
