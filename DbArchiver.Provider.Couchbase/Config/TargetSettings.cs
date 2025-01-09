using DbArchiver.Provider.Common.Config;

namespace DbArchiver.Provider.Couchbase.Config
{
    public class TargetSettings : ITargetSettings
    {
        public string Bucket { get; set; }
        public string KeyProperty { get; set; }
        public ConnectionInfo ConnectionInfo { get; set; }
    }
}
