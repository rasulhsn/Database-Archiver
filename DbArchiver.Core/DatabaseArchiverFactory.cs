using DbArchiver.Core.Factories;
using DbArchiver.Core.Helper;
using DbArchiver.Provider.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DbArchiver.Core
{
    public class DatabaseArchiverFactory : IDatabaseArchiverFactory
    {
        const string PROVIDER_ASSEMBLY_PREFIX = "DbArchiver.Provider.";

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

        public DatabaseArchiver Create()
        {
            ArchiverConfiguration archiverConfiguration = _configurationFactory.Create();

            string sourceAssemblyName = $"{PROVIDER_ASSEMBLY_PREFIX}{archiverConfiguration.Source.Provider}";
            Type sourceType = AssemblyTypeResolver.ResolveByInterface<IDatabaseProviderSource>(sourceAssemblyName);

            string targetAssemblyName = $"{PROVIDER_ASSEMBLY_PREFIX}{archiverConfiguration.Target.Provider}";
            Type targetType = AssemblyTypeResolver.ResolveByInterface<IDatabaseProviderTarget>(targetAssemblyName);

            var providerSource  = _serviceProvider.GetService(sourceType) as IDatabaseProviderSource;
            var providerTarget = _serviceProvider.GetService(sourceType) as IDatabaseProviderTarget;
            var logger = _serviceProvider.GetService<ILogger<DatabaseArchiver>>();

            if (providerSource == null)
                throw new Exception($"Database Source Provider is invalid!");
            
            if (providerTarget == null)
                throw new Exception($"Database Target Provider is invalid!");

            return new DatabaseArchiver(archiverConfiguration,
                                    providerSource,
                                    providerTarget,
                                    logger!);
        }
    }
}
