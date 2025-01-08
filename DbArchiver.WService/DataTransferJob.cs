using DbArchiver.Core;
using Quartz;

namespace DbArchiver.WService
{
    public class DataTransferJob : IJob
    {
        private readonly ILogger<DataTransferJob> _logger;
        private readonly DatabaseArchiver _dbArchiver;

        public DataTransferJob(DatabaseArchiver dbArchiver, ILogger<DataTransferJob> logger)
        { 
            _dbArchiver = dbArchiver;
            _logger = logger;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            _logger.LogInformation("DataTransferJob started at {Time}", DateTimeOffset.Now);

            try
            {
                await _dbArchiver.ArchiveAsync(CancellationToken.None);
                _logger.LogInformation("DataTransferJob completed successfully at {Time}", DateTimeOffset.Now);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while executing DataTransferJob.");
            }
        }
    }
}
