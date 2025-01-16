using System.Text;
using DbArchiver.Provider.Common;
using DbArchiver.Provider.Common.Config;
using DbArchiver.Provider.MySQL.Config;
using MySql.Data.MySqlClient;
using Microsoft.Extensions.Logging;
using Dapper;
namespace DbArchiver.Provider.MySQL;

public class MySQLProvider : IDatabaseProviderSource, IDatabaseProviderTarget
    {
        private readonly ILogger<MySQLProvider> _logger;

        public MySQLProvider(ILogger<MySQLProvider> logger)
        {
            _logger = logger;
        }

        public async Task DeleteAsync(ISourceSettings settings, IEnumerable<object> data)
        {
            if (data == null || !data.Any())
                return;

            var sourceSettings = ResolveSourceSettings(settings);

            using (var connection = new MySqlConnection(sourceSettings.ConnectionString))
            {
                try
                {
                    var dataDictionaries = data.Select(item =>
                        ((IDictionary<string, object>)item).ToDictionary(k => k.Key, v => v.Value));

                    await connection.OpenAsync();

                    foreach (var record in dataDictionaries)
                    {
                        if (!record.ContainsKey(sourceSettings.IdColumn))
                            throw new InvalidOperationException($"Primary key '{sourceSettings.IdColumn}' not found in record.");

                        var query = $"DELETE FROM `{sourceSettings.Schema}`.`{sourceSettings.Table}` WHERE `{sourceSettings.IdColumn}` = @Id";

                        await connection.ExecuteAsync(query, new { Id = record[sourceSettings.IdColumn] });
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error deleting data.");
                    throw;
                }
            }
        }

        public async Task InsertAsync(ITargetSettings settings, IEnumerable<object> data)
        {
            if (data == null || !data.Any())
                return;

            var targetSettings = ResolveTargetSettings(settings);

            using (var connection = new MySqlConnection(targetSettings.ConnectionString))
            {
                try
                {
                    var dataDictionaries = data.Select(item =>
                        ((IDictionary<string, object>)item).ToDictionary(k => k.Key, v => v.Value));
                    
                    await connection.OpenAsync();

                    foreach (var record in dataDictionaries)
                    {
                        var columns = string.Join(", ", record.Keys.Select(k => $"`{k}`"));
                        var parameters = string.Join(", ", record.Keys.Select(k => $"@{k}"));

                        StringBuilder queryStrBuilder = new StringBuilder();
                        queryStrBuilder.Append($"INSERT INTO `{targetSettings.Schema}`.`{targetSettings.Table}` ({columns}) ");
                        queryStrBuilder.Append($"VALUES ({parameters}) ");
                        queryStrBuilder.Append($"ON DUPLICATE KEY UPDATE ");

                        var updateColumns = string.Join(", ", record.Keys.Where(k => k != targetSettings.IdColumn)
                            .Select(k => $"`{k}` = @{k}"));
                        queryStrBuilder.Append(updateColumns);

                        await connection.ExecuteAsync(queryStrBuilder.ToString(), record);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error inserting data.");
                    throw;
                }
            }
        }

        public async Task ExecuteScriptAsync(ITargetSettings settings, string script)
        {
            if (string.IsNullOrEmpty(script))
                throw new ArgumentNullException(nameof(script));

            var sourceSettings = ResolveTargetSettings(settings);

            using (var connection = new MySqlConnection(sourceSettings.ConnectionString))
            {
                try
                {
                    await connection.OpenAsync();
                    await connection.ExecuteAsync(script);
                    await connection.CloseAsync();
                    _logger.LogInformation("Script executed successfully.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error executing script.");
                    throw;
                }
            }
        }

        public async Task<IDatabaseProviderIterator> GetIteratorAsync(ISourceSettings settings, int transferQuantity)
        {
            var sourceSettings = ResolveSourceSettings(settings);

            StringBuilder queryStrBuilder = new StringBuilder();
            queryStrBuilder.AppendLine($"SELECT * FROM `{sourceSettings.Schema}`.`{sourceSettings.Table}`");

            if (sourceSettings.HasCondition)
                queryStrBuilder.AppendLine($"WHERE {sourceSettings.Condition}");

            var connection = new MySqlConnection(sourceSettings.ConnectionString);
            await connection.OpenAsync();

            var iterator = new MySQLProviderIterator(connection,
                                                    queryStrBuilder.ToString(),
                                                    sourceSettings.IdColumn,
                                                    transferQuantity);
            return iterator;
        }

        /// <summary>
        /// Down-cast to MySQL source settings
        /// </summary>
        private SourceSettings ResolveSourceSettings(ISourceSettings sourceSettings)
            => sourceSettings as SourceSettings;

        /// <summary>
        /// Down-cast to MySQL target settings
        /// </summary>
        private TargetSettings ResolveTargetSettings(ITargetSettings targetSettings)
            => targetSettings as TargetSettings;
    }