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
        private readonly IServiceProvider _serviceProvider;
        private readonly IConfiguration _configuration;

        private readonly ILogger<DatabaseArchiverFactory> _logger;

        public DatabaseArchiverFactory(IServiceProvider serviceProvider,
                                        IConfiguration configuration,
                                        ILogger<DatabaseArchiverFactory> logger)
        {
            _serviceProvider = serviceProvider;
            _configuration = configuration;

            _logger = logger;
        }

        public DatabaseArchiver Create(TransferSettings TransferSettings)
        {
            string sourceAssemblyName = $"{Constants.PROVIDER_ASSEMBLY_PREFIX}{TransferSettings.Source.Provider}";
            Type sourceType = AssemblyTypeResolver.ResolveByInterface<IDatabaseProviderSource>(sourceAssemblyName);

            string targetAssemblyName = $"{Constants.PROVIDER_ASSEMBLY_PREFIX}{TransferSettings.Target.Provider}";
            Type targetType = AssemblyTypeResolver.ResolveByInterface<IDatabaseProviderTarget>(targetAssemblyName);

            var providerSource  = _serviceProvider.GetService(sourceType) as IDatabaseProviderSource;
            var providerTarget = _serviceProvider.GetService(targetType) as IDatabaseProviderTarget;          

            if (providerSource == null)
            {
                string errorMessage = "Database Source Provider is invalid!";
                _logger.LogError(errorMessage);
                throw new Exception(errorMessage);
            }
            
            if (providerTarget == null)
            {
                string errorMessage = "Database Target Provider is invalid!";
                _logger.LogError(errorMessage);
                throw new Exception(errorMessage);
            }

            var loggerInstance = _serviceProvider.GetService<ILogger<DatabaseArchiver>>();

            return new DatabaseArchiver(TransferSettings,
                                    providerSource,
                                    providerTarget,
                                    loggerInstance!);
        }
    }
}
