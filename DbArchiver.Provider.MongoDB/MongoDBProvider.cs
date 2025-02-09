using MongoDB.Bson;
using MongoDB.Driver;
using Microsoft.Extensions.Logging;
using DbArchiver.Provider.Common;
using DbArchiver.Provider.Common.Config;
using DbArchiver.Provider.MongoDB.Config;

namespace DbArchiver.Provider.MongoDB
{
    public class MongoDBProvider : IDatabaseProviderSource,
                                    IDatabaseProviderTarget
    {
        private readonly ILogger<MongoDBProvider> _logger;
        private IMongoDatabase _database;

        public MongoDBProvider(ILogger<MongoDBProvider> logger)
        {
            _logger = logger;
        }

        public async Task DeleteAsync(ISourceSettings settings, IEnumerable<object> data)
        {
            if (data == null || !data.Any())
                return;

            try
            {
                var sourceSettings = ResolveSourceSettings(settings);
                await InitializeAsync(sourceSettings.ConnectionString, sourceSettings.DatabaseName);
                
                var collection = _database.GetCollection<BsonDocument>(sourceSettings.Collection);

                var ids = data.Select(d => ((IDictionary<string, object>)d)[sourceSettings.IdColumn]);
                var filter = Builders<BsonDocument>.Filter.In(sourceSettings.IdColumn, ids);

                var result = await collection.DeleteManyAsync(filter);
                _logger.LogInformation($"Deleted {result.DeletedCount} documents.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting data.");
                throw;
            }
        }

        public async Task InsertAsync(ITargetSettings settings, IEnumerable<object> data)
        {
            throw new NotImplementedException();
        }

        public Task ExecuteScriptAsync(ITargetSettings settings, string script)
        {
            throw new NotImplementedException();
        }

        public async Task<IDatabaseProviderIterator> GetIteratorAsync(ISourceSettings settings, int transferQuantity)
        {
            var sourceSettings = ResolveSourceSettings(settings);
            await InitializeAsync(sourceSettings.ConnectionString, sourceSettings.DatabaseName);

            var collection = _database.GetCollection<BsonDocument>(sourceSettings.Collection);

            var iterator = new MongoDBProviderIterator(collection, sourceSettings.IdColumn, transferQuantity);
            return iterator;
        }

        private async Task InitializeAsync(string connectionString, string databaseName)
        {
            if (_database == null)
            {
                var client = new MongoClient(connectionString);
                _database = client.GetDatabase(databaseName);
            }
        }

        private SourceSettings ResolveSourceSettings(ISourceSettings sourceSettings)
            => sourceSettings as SourceSettings;

        private TargetSettings ResolveTargetSettings(ITargetSettings targetSettings)
            => targetSettings as TargetSettings;
    }

}
