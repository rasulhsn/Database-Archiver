using Microsoft.Extensions.DependencyInjection;

namespace DbArchiver.Provider.MySQL;

public static class ServiceCollectionExtension
{
    public static IServiceCollection AddMySQLProviderServices(this IServiceCollection services) {
            
        services.AddTransient<MySQLProvider>();

        return services;
    }
}