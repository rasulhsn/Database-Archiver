using DbArchiver.Core.Common;
using DbArchiver.Core.Config;
using DbArchiver.Core.Helper;
using DbArchiver.Provider.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DbArchiver.Core
{
    public class DatabaseArchiverFactory : IDatabaseArchiverFactory
    {
        private readonly IArchiverConfigurationFactory _configurationFactory;
        private readonly IServiceProvider _serviceProvider;
        private readonly IConfiguration _configuration;

        public DatabaseArchiverFactory(IArchiverConfigurationFactory configurationFactory,
            IServiceProvider serviceProvider,
            IConfiguration configuration)
        {
            _configurationFactory = configurationFactory;
            _serviceProvider = serviceProvider;
            _configuration = configuration;
        }

        public DatabaseArchiver Create(TransferSettings TransferSettings)
        {
            string sourceAssemblyName = $"{Constants.PROVIDER_ASSEMBLY_PREFIX}{TransferSettings.Source.Provider}";
            Type sourceType = AssemblyTypeResolver.ResolveByInterface<IDatabaseProviderSource>(sourceAssemblyName);

            string targetAssemblyName = $"{Constants.PROVIDER_ASSEMBLY_PREFIX}{TransferSettings.Target.Provider}";
            Type targetType = AssemblyTypeResolver.ResolveByInterface<IDatabaseProviderTarget>(targetAssemblyName);

            var providerSource  = _serviceProvider.GetService(sourceType) as IDatabaseProviderSource;
            var providerTarget = _serviceProvider.GetService(sourceType) as IDatabaseProviderTarget;
            var logger = _serviceProvider.GetService<ILogger<DatabaseArchiver>>();

            if (providerSource == null)
                throw new Exception($"Database Source Provider is invalid!");
            if (providerTarget == null)
                throw new Exception($"Database Target Provider is invalid!");

            return new DatabaseArchiver(TransferSettings,
                                    providerSource,
                                    providerTarget,
                                    logger!);
        }
    }
}
