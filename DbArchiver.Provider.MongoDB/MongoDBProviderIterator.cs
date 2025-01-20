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
            var filteredData = await _collection.Find(new BsonDocument())
                                        .Sort(sort)
                                        .Skip(_currentOffset)
                                        .Limit(_batchSize)
                                        .ToListAsync();

            Data = filteredData.Select(d =>
            {
                var dictionary = d.ToDictionary();
                if (dictionary.ContainsKey("_id") && dictionary["_id"] is ObjectId objectId)
                {
                    dictionary["_id"] = objectId.ToString();
                }
                return FlattenDictionary(dictionary);
            });

            _currentOffset += _batchSize;

            return Data.Any();
        }

        private Dictionary<string, object> FlattenDictionary(Dictionary<string, object> source, string parentKey = "")
        {
            var result = new Dictionary<string, object>();

            foreach (var kvp in source)
            {
                var key = string.IsNullOrEmpty(parentKey) ? kvp.Key : $"{parentKey}_{kvp.Key}";

                if (kvp.Value is Dictionary<string, object> nestedDictionary)
                {
                    var flattenedNested = FlattenDictionary(nestedDictionary, key);
                    foreach (var nestedKvp in flattenedNested)
                    {
                        result[nestedKvp.Key] = nestedKvp.Value;
                    }
                }
                else if (kvp.Value is BsonDocument bsonDocument)
                {
                    var nestedFromBson = FlattenDictionary(bsonDocument.ToDictionary(), key);
                    foreach (var nestedKvp in nestedFromBson)
                    {
                        result[nestedKvp.Key] = nestedKvp.Value;
                    }
                }
                else
                {
                    result[key] = kvp.Value;
                }
            }

            return result;
        }

        public void Dispose()
        {
            if (_disposed) return;

            _disposed = true;
        }
    }
}
