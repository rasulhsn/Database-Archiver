using Couchbase.Query;
using Couchbase;
using DbArchiver.Provider.Common;

namespace DbArchiver.Provider.Couchbase
{
    public class CouchbaseProviderIterator : IDatabaseProviderIterator
    {
        private readonly ICluster _cluster;
        private readonly string _bucketName;
        private readonly string _queryStr;
        private readonly string _keyProperty;
        private readonly int _batchSize;

        private int _currentOffset;
        private bool _disposed;

        public IEnumerable<object> Data { get; private set; }

        public CouchbaseProviderIterator(ICluster cluster, string bucketName, string query, string keyProperty, int batchSize)
        {
            _cluster = cluster;
            _bucketName = bucketName;
            _queryStr = query;
            _keyProperty = keyProperty;
            _batchSize = batchSize;
            _currentOffset = 0;
            _disposed = false;
        }

        public async Task<bool> NextAsync()
        {
            if (_disposed) return false;

            var paginatedQuery = $@"
            {_queryStr}
            OFFSET {_currentOffset} LIMIT {_batchSize}";

            try
            {
                var queryResult = await _cluster.QueryAsync<dynamic>(paginatedQuery);

                if (queryResult.MetaData.Status != QueryStatus.Success)
                {
                    throw new InvalidOperationException("Failed to execute paginated query.");
                }

                Data = await queryResult.Rows.Select(row => (object)row).ToListAsync();
                _currentOffset += _batchSize;

                return Data.Any();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error during data retrieval.", ex);
            }
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
        }
    }
}
