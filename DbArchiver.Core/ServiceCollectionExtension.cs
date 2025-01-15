using DbArchiver.Core.Common;
using DbArchiver.Core.Config;
using Microsoft.Extensions.DependencyInjection;

namespace DbArchiver.Core
{
    public static class ServiceCollectionExtension
    {
        public static IServiceCollection AddCoreServices(this IServiceCollection services) {

            services.AddSingleton<IArchiverConfigurationFactory, ArchiverConfigurationFactory>();
            services.AddSingleton<IDatabaseArchiverFactory, DatabaseArchiverFactory>();

            return services;
        }
    }
}
