using DbArchiver.Core;
using DbArchiver.Provider.MSSQL;
using DbArchiver.Provider.SQLite;
using DbArchiver.Provider.PostgreSQL;
using Quartz;

namespace DbArchiver.WService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = Host.CreateApplicationBuilder(args);
            
            builder.Services.AddMSSQLProviderServices();
            builder.Services.AddPostgreSQLProviderServices();
            builder.Services.AddSQLiteProviderServices();
            builder.Services.AddCoreServices();
            
            var configuration = builder.Configuration;

            builder.Services.AddQuartz(q =>
            {
                q.UseMicrosoftDependencyInjectionJobFactory();

                var jobKey = new JobKey(configuration["JobConfiguration:Name"]);

                q.AddJob<DataTransferJob>(opts => opts.WithIdentity(jobKey));

                q.AddTrigger(opts => opts
                        .ForJob(jobKey)
                        .WithIdentity("DataTransferTrigger")
                        .WithCronSchedule(configuration["JobConfiguration:Cron"])
                    );
            });

            builder.Services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);

            var host = builder.Build();
            host.Run();  
        }
    }
}