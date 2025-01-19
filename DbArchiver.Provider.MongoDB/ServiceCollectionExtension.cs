using Microsoft.Extensions.DependencyInjection;

namespace DbArchiver.Provider.MongoDB
{
    public static class ServiceCollectionExtension
    {
        public static IServiceCollection AddMongoDBProviderServices(this IServiceCollection services) {
            
            services.AddTransient<MongoDBProvider>();

            return services;
        }
    }
}
