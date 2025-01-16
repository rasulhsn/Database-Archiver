using Dapper;
using DbArchiver.Provider.Common;
using System.Data;

namespace DbArchiver.Provider.SQLite
{
    public class SQLiteProviderIterator(
        IDbConnection connection,
        string query,
        string orderByColumn,
        int batchSize)
        : IDatabaseProviderIterator
    {
        private int _currentOffset = 0;

        private bool _disposed = false;

        public IEnumerable<object> Data { get; private set; }

        public async Task<bool> NextAsync()
        {
            if (_disposed) return false;

            var paginatedQuery = $@"
                {query}
                ORDER BY {orderByColumn}
                LIMIT @batchSize OFFSET @currentOffset";

            var dynamicParams = new DynamicParameters();
            dynamicParams.Add("currentOffset", _currentOffset);
            dynamicParams.Add("batchSize", batchSize);

            Data = (await connection.QueryAsync(paginatedQuery, dynamicParams)).ToList();
            _currentOffset += batchSize;

            return Data.Any();
        }

        public void Dispose()
        {
            if (_disposed) return;

            connection?.Dispose();
            _disposed = true;
        }
    }

}
