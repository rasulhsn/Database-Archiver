using DbArchiver.Core.Contract;
using DbArchiver.Core.Config;
using Quartz;

namespace DbArchiver.WService
{
    public class DataTransferJob : IJob
    {
        private readonly IDatabaseArchiverFactory _archiverFactory;
        private readonly ILogger<DataTransferJob> _logger;
        
        public DataTransferJob(IDatabaseArchiverFactory dbArchiverFactory, ILogger<DataTransferJob> logger)
        {
            _archiverFactory = dbArchiverFactory ?? throw new ArgumentNullException(nameof(dbArchiverFactory));
            _logger = logger;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            _logger.LogInformation("DataTransferJob started at {Time}", DateTimeOffset.Now);

            try
            {
                var transferSettings = context.MergedJobDataMap.Get("TransferSettings") as TransferSettings;

                if (transferSettings == null)
                {
                    _logger.LogError("TransferSettings not provided to job.");
                    return;
                }

                var archiver = _archiverFactory.Create(transferSettings);
                await archiver.ArchiveAsync(CancellationToken.None);

                _logger.LogInformation("DataTransferJob completed successfully at {Time}", DateTimeOffset.Now);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while executing DataTransferJob.");
            }
        }
    }
}
