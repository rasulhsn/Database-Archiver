using DbArchiver.Provider.Common;
using Microsoft.Extensions.DependencyInjection;

namespace DbArchiver.Provider.MSSQL
{
    public static class ServiceCollectionExtension
    {
        public static IServiceCollection AddMSSQLProviderServices(this IServiceCollection services) {
            
            services.AddTransient<MSSQLProvider>();

            return services;
        }
    }
}
