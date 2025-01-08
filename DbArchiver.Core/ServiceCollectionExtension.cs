using DbArchiver.Core.Factories;
using Microsoft.Extensions.DependencyInjection;

namespace DbArchiver.Core
{
    public static class ServiceCollectionExtension
    {
        public static IServiceCollection AddCoreServices(this IServiceCollection services) {

            services.AddSingleton<IArchiverConfigurationFactory, ArchiverConfigurationFactory>();
            services.AddSingleton<IDatabaseArchiverFactory, DatabaseArchiverFactory>();

            services.AddTransient<DatabaseArchiver>(provider =>
            {
                var factory = provider.GetService<IDatabaseArchiverFactory>();
                return factory!.Create();
            });

            return services;
        }
    }
}
