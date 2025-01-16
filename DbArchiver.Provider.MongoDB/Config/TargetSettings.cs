using DbArchiver.Provider.Common.Config;

namespace DbArchiver.Provider.MongoDB.Config
{
    public class TargetSettings : ITargetSettings
    {
        public string ConnectionString { get; set; }
        public string DatabaseName { get; set; }
        public string Collection { get; set; }
        public string IdColumn { get; set; }
    }
}
