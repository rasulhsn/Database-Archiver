
using DbArchiver.Core.Config;

namespace DbArchiver.Core.Common
{
    public interface IDatabaseArchiverFactory
    {
        DatabaseArchiver Create(TransferSettings TransferSettings);
    }
}
