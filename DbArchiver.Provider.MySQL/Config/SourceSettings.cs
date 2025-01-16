using DbArchiver.Provider.Common.Config;

namespace DbArchiver.Provider.MySQL.Config;

public class SourceSettings : ISourceSettings
{
    public string ConnectionString { get; set; }
    public string Schema { get; set; }
    public string Table { get; set; }
    public string IdColumn { get; set; }
    public string Condition { get; set; }

    public bool HasCondition => !string.IsNullOrEmpty(Condition);
}