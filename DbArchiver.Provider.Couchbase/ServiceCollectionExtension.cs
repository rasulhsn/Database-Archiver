using Microsoft.Extensions.DependencyInjection;

namespace DbArchiver.Provider.Couchbase
{
    public static class ServiceCollectionExtension
    {
        public static IServiceCollection AddCouchbaseProviderServices(this IServiceCollection services) {
            
            services.AddTransient<CouchbaseProvider>();

            return services;
        }
    }
}
