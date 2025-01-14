using DbArchiver.Provider.Common.Config;

namespace DbArchiver.Core.Config
{
    public class TransferSettings
    {
        public SourceProviderSettings Source { get; set; }
        public TargetProviderSettings Target { get; set; }
    }

    public class SourceProviderSettings
    {
        public string Provider { get; set; }
        public string Host { get; set; }
        public int TransferQuantity { get; set; }
        public bool DeleteAfterArchived { get; set; }
        public ISourceSettings Settings { get; set; }
    }

    public class TargetProviderSettings
    {
        public string Provider { get; set; }
        public string Host { get; set; }
        public string PreScript { get; set; }
        public bool HasPreScript => !string.IsNullOrEmpty(PreScript);
        public ITargetSettings Settings { get; set; }
    }
}