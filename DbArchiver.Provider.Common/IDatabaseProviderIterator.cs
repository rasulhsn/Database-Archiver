
namespace DbArchiver.Provider.Common
{
    public interface IDatabaseProviderIterator<T> : IDisposable
    {
        T Data { get; }
        Task<bool> NextAsync();
    }

    public interface IDatabaseProviderIterator : IDatabaseProviderIterator<IEnumerable<object>>
    {
    }
}
