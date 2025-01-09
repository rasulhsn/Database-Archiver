using Microsoft.Extensions.DependencyInjection;

namespace DbArchiver.Provider.SQLite
{
    public static class ServiceCollectionExtension
    {
        public static IServiceCollection AddMSSQLProviderServices(this IServiceCollection services) {
            
            services.AddTransient<SQLiteProvider>();

            return services;
        }
    }
}
