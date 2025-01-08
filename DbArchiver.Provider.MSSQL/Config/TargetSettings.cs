using DbArchiver.Provider.Common.Config;

namespace DbArchiver.Provider.MSSQL.Config
{
    public class TargetSettings : ITargetSettings
    {
        public string ConnectionString { get; set; }
        public string Schema { get; set; }
        public string Table { get; set; }
        public string IdColumn { get; set; }
        public string Condition { get; set; }

        public bool HasCondition => !string.IsNullOrEmpty(Condition);
    }
}
