using Couchbase;
using Couchbase.Query;
using DbArchiver.Provider.Common;
using DbArchiver.Provider.Common.Config;
using DbArchiver.Provider.Couchbase.Config;
using Microsoft.Extensions.Logging;
using System.Text;

namespace DbArchiver.Provider.Couchbase
{
    public class CouchbaseProvider : IDatabaseProviderSource,
                                  IDatabaseProviderTarget
    {
        private readonly ILogger<CouchbaseProvider> _logger;
        private readonly IBucket _bucket;

        public CouchbaseProvider(ILogger<CouchbaseProvider> logger, ICluster cluster, string bucketName)
        {
            _logger = logger;
            _bucket = cluster.BucketAsync(bucketName).Result;
        }

        public async Task DeleteAsync(ISourceSettings settings, IEnumerable<object> data)
        {
            if (data == null || !data.Any())
                return;

            var sourceSettings = ResolveSourceSettings(settings);

            try
            {
                var dataDictionaries = data.Select(item =>
                    ((IDictionary<string, object>)item).ToDictionary(k => k.Key, v => v.Value));

                foreach (var record in dataDictionaries)
                {
                    if (!record.ContainsKey(sourceSettings.KeyProperty))
                        throw new InvalidOperationException($"Primary key '{sourceSettings.KeyProperty}' not found in record.");

                    var documentId = record[sourceSettings.KeyProperty].ToString();
                    await _bucket.DefaultCollection().RemoveAsync(documentId);

                    _logger.LogInformation("Deleted document with ID: {DocumentId}", documentId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting data.");
                throw;
            }
        }

        public async Task InsertAsync(ITargetSettings settings, IEnumerable<object> data)
        {
            if (data == null || !data.Any())
                return;

            var targetSettings = ResolveTargetSettings(settings);

            try
            {
                var dataDictionaries = data.Select(item =>
                    ((IDictionary<string, object>)item).ToDictionary(k => k.Key, v => v.Value));

                foreach (var record in dataDictionaries)
                {
                    if (!record.ContainsKey(targetSettings.KeyProperty))
                        throw new InvalidOperationException($"Primary key '{targetSettings.KeyProperty}' not found in record.");

                    var documentId = record[targetSettings.KeyProperty].ToString();
                    await _bucket.DefaultCollection().UpsertAsync(documentId, record);

                    _logger.LogInformation("Inserted or updated document with ID: {DocumentId}", documentId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inserting data.");
                throw;
            }
        }

        public async Task ExecuteScriptAsync(ITargetSettings settings, string script)
        {
            if (string.IsNullOrEmpty(script))
                throw new ArgumentNullException(nameof(script));

            try
            {
                var cluster = _bucket.Cluster;
                var queryResult = await cluster.QueryAsync<dynamic>(script);

                if (!queryResult.MetaData.Status.Equals(QueryStatus.Success))
                {
                    _logger.LogError("Script execution failed: {Errors}", queryResult.Errors);
                    throw new InvalidOperationException("Script execution failed.");
                }

                _logger.LogInformation("Script executed successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing script.");
                throw;
            }
        }

        public async Task<IDatabaseProviderIterator> GetIteratorAsync(ISourceSettings settings, int transferQuantity)
        {
            var sourceSettings = ResolveSourceSettings(settings);

            try
            {
                var queryBuilder = new StringBuilder();
                queryBuilder.AppendLine($"SELECT * FROM `{sourceSettings.Bucket}` WHERE {sourceSettings.KeyProperty} IS NOT NULL");

                var cluster = _bucket.Cluster;

                return new CouchbaseProviderIterator(cluster,
                            sourceSettings.Bucket,
                            queryBuilder.ToString(),
                            sourceSettings.KeyProperty,
                            transferQuantity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching data iterator.");
                throw;
            }
        }

        private SourceSettings ResolveSourceSettings(ISourceSettings sourceSettings)
            => sourceSettings as SourceSettings;

        private TargetSettings ResolveTargetSettings(ITargetSettings targetSettings)
            => targetSettings as TargetSettings;
    }

}
