
using DbArchiver.Core.Config;

namespace DbArchiver.Core.Contract
{
    public interface IDatabaseArchiverFactory
    {
        DatabaseArchiver Create(TransferSettings TransferSettings);
    }
}
