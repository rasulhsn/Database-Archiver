using Microsoft.Extensions.DependencyInjection;

namespace DbArchiver.Provider.PostgreSQL
{
    public static class ServiceCollectionExtension
    {
        public static IServiceCollection AddMSSQLProviderServices(this IServiceCollection services) {
            
            services.AddTransient<PostgreSQLProvider>();

            return services;
        }
    }
}
