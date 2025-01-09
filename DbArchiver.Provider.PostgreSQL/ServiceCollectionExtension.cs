using Microsoft.Extensions.DependencyInjection;

namespace DbArchiver.Provider.PostgreSQL
{
    public static class ServiceCollectionExtension
    {
        public static IServiceCollection AddPostgreSQLProviderServices(this IServiceCollection services) {
            
            services.AddTransient<PostgreSQLProvider>();

            return services;
        }
    }
}
