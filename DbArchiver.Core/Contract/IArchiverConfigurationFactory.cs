using DbArchiver.Core.Config;

namespace DbArchiver.Core.Contract
{
    public interface IArchiverConfigurationFactory
    {
        ArchiverConfiguration Create();
    }
}
