using DbArchiver.Provider.Common;
using MongoDB.Bson;
using MongoDB.Driver;

namespace DbArchiver.Provider.MongoDB
{
    public class MongoDBProviderIterator : IDatabaseProviderIterator
    {
        private readonly IMongoCollection<BsonDocument> _collection;
        private readonly string _orderByColumn;
        private readonly int _batchSize;
        private int _currentOffset;

        private bool _disposed;
        public IEnumerable<object> Data { get; private set; }

        public MongoDBProviderIterator(IMongoCollection<BsonDocument> collection, string orderByColumn, int batchSize)
        {
            _collection = collection;
            _orderByColumn = orderByColumn;
            _batchSize = batchSize;
            _currentOffset = 0;
            _disposed = false;
        }

        public async Task<bool> NextAsync()
        {
            if (_disposed) return false;

            var sort = Builders<BsonDocument>.Sort.Ascending(_orderByColumn);
            var data = await _collection.Find(new BsonDocument())
                                        .Sort(sort)
                                        .Skip(_currentOffset)
                                        .Limit(_batchSize)
                                        .ToListAsync();

            Data = data.Select(d => d.ToDictionary());
            _currentOffset += _batchSize;

            return Data.Any();
        }

        public void Dispose()
        {
            if (_disposed) return;

            _disposed = true;
        }
    }

}
