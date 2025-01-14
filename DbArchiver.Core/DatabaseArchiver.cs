using DbArchiver.Core.Config;
using DbArchiver.Core.Helper;
using DbArchiver.Provider.Common;
using Microsoft.Extensions.Logging;

namespace DbArchiver.Core
{
    public class DatabaseArchiver
    {
        private readonly IDatabaseProviderSource _providerSource;
        private readonly IDatabaseProviderTarget _providerTarget;
        private readonly TransferSettings _transferSettings;

        private readonly ILogger<DatabaseArchiver> _logger;

        public DatabaseArchiver(TransferSettings transferSettings,
                            IDatabaseProviderSource source,
                            IDatabaseProviderTarget target,
                            ILogger<DatabaseArchiver> logger)
        {
            _transferSettings = transferSettings;
            _providerSource = source;
            _providerTarget = target;
       
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
                if (_transferSettings.Target.HasPreScript)
                    await _providerTarget.ExecuteScriptAsync(_transferSettings.Target.Settings,
                                                                    _transferSettings.Target.PreScript);

                iterator = await _providerSource.GetIteratorAsync(_transferSettings.Source.Settings,
                                                                    _transferSettings.Source.TransferQuantity);
                while ((await iterator.NextAsync()))
                {
                    // should be transactional!
                    await _providerTarget.InsertAsync(_transferSettings.Target.Settings, iterator.Data);

                    if (_transferSettings.Source.DeleteAfterArchived)
                        await _providerSource.DeleteAsync(_transferSettings.Source.Settings, iterator.Data);

                    _logger.LogDebug($"{nameof(DatabaseArchiver.ArchiveAsync)} archived {_transferSettings.Source.TransferQuantity} data!");
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
            IpPinger.Ping(_transferSettings.Source.Host);
            IpPinger.Ping(_transferSettings.Target.Host);
        }
    }
}
