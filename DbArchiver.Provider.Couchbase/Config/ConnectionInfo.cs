
namespace DbArchiver.Provider.Couchbase.Config
{
    public sealed class ConnectionInfo
    {
        public string Hosts { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public int UiPort { get; set; }
    }
}
