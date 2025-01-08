using Dapper;
using DbArchiver.Provider.Common;
using DbArchiver.Provider.Common.Config;
using DbArchiver.Provider.MSSQL.Config;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using System.Text;
using static Dapper.SqlMapper;

namespace DbArchiver.Provider.MSSQL
{
    public class MSSQLProvider : IDatabaseProviderSource,
                                    IDatabaseProviderTarget
    {
        private readonly ILogger<MSSQLProvider> _logger;

        public MSSQLProvider(ILogger<MSSQLProvider> logger)
        {
            _logger = logger;
        }

        public async Task DeleteAsync(ISourceSettings settings, IEnumerable<object> data)
        {
            if (data == null || !data.Any())
                return;

            var sourceSettings = ResolveSourceSettings(settings);

            using (var connection = new SqlConnection(sourceSettings.ConnectionString))
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

                        var query = $"DELETE FROM {sourceSettings.Schema}.{sourceSettings.Table} WHERE {sourceSettings.IdColumn} = @Id";

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

            using (var connection = new SqlConnection(targetSettings.ConnectionString))
            {
                try
                {
                    var dataDictionaries = data.Select(item =>
                        ((IDictionary<string, object>)item).ToDictionary(k => k.Key, v => v.Value));
                    
                    await connection.OpenAsync();

                    foreach (var record in dataDictionaries)
                    {
                        var columns = string.Join(", ", record.Keys);
                        var parameters = string.Join(", ", record.Keys.Select(k => $"@{k}"));

                        StringBuilder queryStrBuilder = new StringBuilder();
                        queryStrBuilder.Append($"IF NOT EXISTS (SELECT {targetSettings.IdColumn} FROM {targetSettings.Schema}.{targetSettings.Table} WHERE {targetSettings.IdColumn} = @{targetSettings.IdColumn}) ");
                        queryStrBuilder.Append($"INSERT INTO {targetSettings.Schema}.{targetSettings.Table} ({columns}) VALUES ({parameters})");

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

            using (var connection = new SqlConnection(sourceSettings.ConnectionString))
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
            queryStrBuilder.AppendLine($"SELECT * FROM {sourceSettings.Schema}.{sourceSettings.Table}");

            if (sourceSettings.HasCondition)
                queryStrBuilder.AppendLine($"WHERE {sourceSettings.Condition}");
            
            var connection = new SqlConnection(sourceSettings.ConnectionString);
            await connection.OpenAsync();

            var iterator = new MSSQLProviderIterator(connection,
                                                    queryStrBuilder.ToString(),
                                                    sourceSettings.IdColumn,
                                                    transferQuantity);
            return iterator;
        }

        /// <summary>
        /// Down-cast to mssql source settings
        /// </summary>
        private SourceSettings ResolveSourceSettings(ISourceSettings sourceSettings)
            => sourceSettings as SourceSettings;

        /// <summary>
        /// Down-cast to mssql target settings
        /// </summary>
        private TargetSettings ResolveTargetSettings(ITargetSettings targetSettings)
            => targetSettings as TargetSettings;
    }
}
