using DbArchiver.Provider.Common;
using DbArchiver.Provider.Common.Config;

namespace DbArchiver.Core
{
    public class ArchiverConfiguration
    {
        public SourceConfiguration Source { get; set; }
        public TargetConfiguration Target { get; set; }
    }

    public class SourceConfiguration
    {
        public string Provider { get; set; }
        public string Host { get; set; }
        public int TransferQuantity { get; set; }
        public bool DeleteAfterArchived { get; set; }
        public ISourceSettings Settings { get; set; }
    }

    public class TargetConfiguration
    {
        public string Provider { get; set; }
        public string Host { get; set; }
        public string PreScript { get; set; }
        public bool HasPreScript => !string.IsNullOrEmpty(PreScript);
        public ITargetSettings Settings { get; set; }
    }
}