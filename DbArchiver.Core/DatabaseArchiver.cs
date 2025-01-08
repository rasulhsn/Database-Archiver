using DbArchiver.Core.Helper;
using DbArchiver.Provider.Common;
using Microsoft.Extensions.Logging;

namespace DbArchiver.Core
{
    public class DatabaseArchiver
    {
        private readonly IDatabaseProviderSource _providerSource;
        private readonly IDatabaseProviderTarget _providerTarget;
        private readonly ArchiverConfiguration _configuration;

        private readonly ILogger<DatabaseArchiver> _logger;

        public DatabaseArchiver(ArchiverConfiguration configuration,
                            IDatabaseProviderSource source,
                            IDatabaseProviderTarget target,
                            ILogger<DatabaseArchiver> logger)
        {
            _providerSource = source;
            _providerTarget = target;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task ArchiveAsync(CancellationToken cancellationToken)
        {
            _logger.LogDebug($"{nameof(DatabaseArchiver.ArchiveAsync)} started!");

            PingHosts();

            IDatabaseProviderIterator iterator = null;

            try
            {
                // execute pre-script
                if (_configuration.Target.HasPreScript)
                    await _providerTarget.ExecuteScriptAsync(_configuration.Target.Settings,
                                                                    _configuration.Target.PreScript);

                iterator = await _providerSource.GetIteratorAsync(_configuration.Source.Settings,
                                                                    _configuration.Source.TransferQuantity);
                while ((await iterator.NextAsync()))
                {
                    // should be transactional!
                    await _providerTarget.InsertAsync(_configuration.Target.Settings, iterator.Data);

                    if (_configuration.Source.DeleteAfterArchived)
                        await _providerSource.DeleteAsync(_configuration.Source.Settings, iterator.Data);

                    _logger.LogDebug($"{nameof(DatabaseArchiver.ArchiveAsync)} archived {_configuration.Source.TransferQuantity} data!");
                }           
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw;
            }
            finally
            {
                iterator?.Dispose();
            }

            _logger.LogDebug($"{nameof(DatabaseArchiver.ArchiveAsync)} Finished!");
        }

        private void PingHosts()
        {
            IpPinger.Ping(_configuration.Source.Host);
            IpPinger.Ping(_configuration.Target.Host);
        }
    }
}
