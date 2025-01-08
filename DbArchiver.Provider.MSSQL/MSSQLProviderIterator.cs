using Dapper;
using DbArchiver.Provider.Common;
using System.Data;

namespace DbArchiver.Provider.MSSQL
{
    public class MSSQLProviderIterator : IDatabaseProviderIterator
    {
        private readonly IDbConnection _connection;
        private readonly string _queryStr;
        private readonly string _orderByColumn;
        private readonly int _batchSize;
        private int _currentOffset;

        public IEnumerable<object> Data { get; private set; }

        public MSSQLProviderIterator(IDbConnection connection,
                        string query, string orderByColumn,int batchSize)
        {
            _connection = connection;
            _queryStr = query;
            _orderByColumn = orderByColumn;
            _batchSize = batchSize;
            _currentOffset = 0;
        }

        public async Task<bool> NextAsync()
        {
            var paginatedQuery = $@"
                    {_queryStr}
                    ORDER BY {_orderByColumn} 
                    OFFSET @currentOffset ROWS FETCH NEXT @batchSize ROWS ONLY";

            var dynamicParams = new DynamicParameters();
            dynamicParams.Add("currentOffset", _currentOffset);
            dynamicParams.Add("batchSize", _batchSize);

            Data = (await _connection.QueryAsync(paginatedQuery, dynamicParams)).ToList();
            _currentOffset += _batchSize;

            return Data.Any();
        }

        public void Dispose()
        {
            _connection?.Dispose();
        }
    }
}
