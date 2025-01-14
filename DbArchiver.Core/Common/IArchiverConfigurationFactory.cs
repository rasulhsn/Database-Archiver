using DbArchiver.Core.Config;

namespace DbArchiver.Core.Common
{
    public interface IArchiverConfigurationFactory
    {
        ArchiverConfiguration Create();
    }
}
