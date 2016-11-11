using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Table;
using NuGet.Versioning;

namespace NetStandardTypes
{
    public sealed class PackageTable
    {
        private readonly CloudTable _table = GetTable();

        private static CloudTable GetTable()
        {
            return Config.CreateCloudTableClient().GetTableReference(Config.PackageTableName);
        }

        public static Task InitializeAsync() => GetTable().CreateIfNotExistsAsync();

        public async Task<VersionCommit> TryGetVersionCommitAsync(string id, string version)
        {
            var entity = await Entity.FindOrDefaultAsync(_table, id, NuGetVersion.Parse(version).IsPrerelease).ConfigureAwait(false);
            if (entity == null)
                return null;
            return new VersionCommit
            {
                Version = entity.Version,
                CommitId = entity.CommitId,
            };
        }

        public Task SetVersionAsync(string id, NuGetVersion version, string commitId)
        {
            var entity = new Entity(_table, id, version)
            {
                Version = version.ToString(),
                CommitId = commitId,
                Processed = false,
            };
            return entity.InsertOrReplaceAsync();
        }

        public Task MarkProcessedAsync(string id, string version)
        {
            var entity = new Entity(_table, id, version)
            {
                Processed = true,
            };
            return entity.InsertOrReplaceAsync();
        }

        private sealed class Entity : TableEntityBase
        {
            public Entity(CloudTable table, string id, string version)
                : this(table, id, NuGetVersion.Parse(version))
            {
            }

            public Entity(CloudTable table, string id, NuGetVersion version)
                : base(table, ToPartitionKey(id), ToRowKey(version.IsPrerelease))
            {
            }

            private Entity(CloudTable table, DynamicTableEntity entity)
                : base(table, entity)
            {
            }

            private static string ToPartitionKey(string id) => id.ToLowerInvariant();

            private static string ToRowKey(bool isPrerelease) => isPrerelease ? "1" : "0";

            public static async Task<Entity> FindOrDefaultAsync(CloudTable table, string id, bool isPrerelease)
            {
                var entity = await FindOrDefaultAsync(table, ToPartitionKey(id), ToRowKey(isPrerelease)).ConfigureAwait(false);
                if (entity == null)
                    return null;
                return new Entity(table, entity);
            }

            public string Version
            {
                get { return Get(); }
                set { Set(value.ToLowerInvariant()); }
            }

            public bool Processed
            {
                get { return Get("0") == "1"; }
                set { Set(value ? "1" : "0"); }
            }

            public string CommitId
            {
                get {  return Get();}
                set {  Set(value.ToLowerInvariant());}
            }
        }

        public sealed class VersionCommit
        {
            public string Version { get; set; }
            public string CommitId { get; set; }
        }
    }
}
