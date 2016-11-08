using System;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Table;

namespace NetStandardTypes
{
    public abstract class TableEntityBase
    {
        private readonly CloudTable _table;
        private readonly DynamicTableEntity _entity;

        protected TableEntityBase(CloudTable table, string partitionKey, string rowKey)
        {
            _table = table;
            _entity = new DynamicTableEntity();
            PartitionKey = partitionKey;
            RowKey = rowKey;
        }

        protected TableEntityBase(CloudTable table, DynamicTableEntity entity)
        {
            _table = table;
            _entity = entity;
        }

        /// <summary>
        /// Performs a point search in an azure table, returning either a <see cref="DynamicTableEntity"/> or <c>null</c>.
        /// </summary>
        /// <param name="table">The azure table.</param>
        /// <param name="partitionKey">The partition key.</param>
        /// <param name="rowKey">The row key.</param>
        public static async Task<DynamicTableEntity> FindOrDefaultAsync(CloudTable table, string partitionKey, string rowKey)
        {
            var tr = await table.ExecuteAsync(TableOperation.Retrieve(partitionKey, rowKey)).ConfigureAwait(false);
            if ((HttpStatusCode)tr.HttpStatusCode == HttpStatusCode.NotFound)
                return null;
            return (DynamicTableEntity)tr.Result;
        }

        protected string ETag
        {
            get { return _entity.ETag; }
            set { _entity.ETag = value; }
        }

        protected string PartitionKey
        {
            get { return _entity.PartitionKey; }
            set { _entity.PartitionKey = value; }
        }

        protected string RowKey
        {
            get { return _entity.RowKey; }
            set { _entity.RowKey = value; }
        }

        protected DateTimeOffset Timestamp
        {
            get { return _entity.Timestamp; }
            set { _entity.Timestamp = value; }
        }

        protected string Get(string defaultValue = null, [CallerMemberName] string propertyName = null) => GetProperty(defaultValue, propertyName);

        protected string GetProperty(string defaultValue, string propertyName)
        {
            EntityProperty result;
            if (_entity.Properties.TryGetValue(propertyName, out result))
                return result.StringValue;
            return defaultValue;
        }

        protected void Set(string value, [CallerMemberName] string propertyName = null) => SetProperty(value, propertyName);

        protected void SetProperty(string value, string propertyName)
        {
            _entity.Properties[propertyName] = EntityProperty.GeneratePropertyForString(value);
        }

        public Task InsertOrReplaceAsync()
        {
            return _table.ExecuteAsync(TableOperation.InsertOrReplace(_entity));
        }

        protected DynamicTableEntity Entity => _entity;
    }
}
