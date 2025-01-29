﻿using DbArchiver.Core;
using DbArchiver.Provider.MSSQL;
using DbArchiver.Provider.SQLite;
using DbArchiver.Provider.PostgreSQL;
using DbArchiver.Provider.MySQL;
using Quartz;
using DbArchiver.Core.Contract;
using DbArchiver.Provider.MongoDB;

namespace DbArchiver.WService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = Host.CreateApplicationBuilder(args);

            builder.Services.AddMongoDBProviderServices();
            builder.Services.AddMSSQLProviderServices();
            builder.Services.AddPostgreSQLProviderServices();
            builder.Services.AddSQLiteProviderServices();
            builder.Services.AddMySQLProviderServices();
            builder.Services.AddCoreServices();

            builder.Services.AddQuartz(q =>
            {
                q.UseMicrosoftDependencyInjectionJobFactory();

                using var serviceProvider = builder.Services.BuildServiceProvider();
                var factory = serviceProvider.GetRequiredService<IArchiverConfigurationFactory>();

                var archiverConfig = factory.Create();

                if (archiverConfig?.Items != null)
                {
                    foreach (var item in archiverConfig.Items)
                    {
                        var jobKey = new JobKey(item.JobSchedulerSettings.JobName);

                        q.AddJob<DataTransferJob>(opts => opts.WithIdentity(jobKey));

                        q.AddTrigger(opts => opts
                            .ForJob(jobKey)
                            .UsingJobData(new JobDataMap
                            {
                                { "TransferSettings", item.TransferSettings }
                            })
                            .WithIdentity($"{item.JobSchedulerSettings.JobName}_Trigger")
                            .WithCronSchedule(item.JobSchedulerSettings.Cron));
                    }
                }
            });

            builder.Services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);

            var host = builder.Build();
            host.Run();  
        }
    }
}