
namespace DbArchiver.Core.Config
{
    public class ArchiverConfiguration
    {
        public IEnumerable<ArchiverConfigurationItem> Items { get; set; }
    }

    public class ArchiverConfigurationItem
    {
        public JobSchedulerSettings JobSchedulerSettings { get; set; }
        public TransferSettings TransferSettings { get; set; }
    }
}
