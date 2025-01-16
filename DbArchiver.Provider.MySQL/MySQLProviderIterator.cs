using Dapper;
using System.Data;
using DbArchiver.Provider.Common;

namespace DbArchiver.Provider.MySQL;

public class MySQLProviderIterator : IDatabaseProviderIterator
{
    private readonly IDbConnection _connection;
    private readonly string _queryStr;
    private readonly string _orderByColumn;
    private readonly int _batchSize;
    private int _currentOffset;

    private bool _disposed;

    public IEnumerable<object> Data { get; private set; }

    public MySQLProviderIterator(IDbConnection connection, string query, string orderByColumn, int batchSize)
    {
        _connection = connection;
        _queryStr = query;
        _orderByColumn = orderByColumn;
        _batchSize = batchSize;
        _currentOffset = 0;
        _disposed = false;
    }

    public async Task<bool> NextAsync()
    {
        if (_disposed) return false;

        // MySQL uses LIMIT with OFFSET for pagination
        var paginatedQuery = $@"{_queryStr}
                                ORDER BY `{_orderByColumn}`
                                LIMIT @batchSize OFFSET @currentOffset";

        var dynamicParams = new DynamicParameters();
        dynamicParams.Add("batchSize", _batchSize);
        dynamicParams.Add("currentOffset", _currentOffset);

        Data = (await _connection.QueryAsync(paginatedQuery, dynamicParams)).ToList();
        _currentOffset += _batchSize;

        return Data.Any();
    }

    public void Dispose()
    {
        if (_disposed) return;

        _connection?.Dispose();
        _disposed = true;
    }
}