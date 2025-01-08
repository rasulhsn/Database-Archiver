using System.Net.NetworkInformation;

namespace DbArchiver.Core.Helper
{
    public static class IpPinger
    {
        public static void Ping(string hostIp, int timeout = 3000)
        {
            var ping = new Ping();

            var pingReply = ping.Send(hostIp, timeout);
            if (pingReply?.Status != IPStatus.Success)
            {
                throw new Exception($"{hostIp} is unreachable");
            }
        }
    }
}
