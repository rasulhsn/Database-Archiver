using DbArchiver.Provider.Common.Config;

namespace DbArchiver.Provider.Common
{
    public interface IDatabaseProviderSource
    {
        Task<IDatabaseProviderIterator> GetIteratorAsync(ISourceSettings settings, int transferQuantity);
        Task DeleteAsync(ISourceSettings settings, IEnumerable<object> data);
    }
}
