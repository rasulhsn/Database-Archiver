using DbArchiver.Provider.Common.Config;

namespace DbArchiver.Provider.Common
{
    public interface IDatabaseProviderTarget
    {
        Task ExecuteScriptAsync(ITargetSettings settings, string script);
        Task InsertAsync(ITargetSettings settings, IEnumerable<object> data);
    }
}
