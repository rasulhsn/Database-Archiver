using Microsoft.Extensions.DependencyInjection;

namespace DbArchiver.Provider.Couchbase
{
    public static class ServiceCollectionExtension
    {
        public static IServiceCollection AddMSSQLProviderServices(this IServiceCollection services) {
            
            services.AddTransient<CouchbaseProvider>();

            return services;
        }
    }
}
