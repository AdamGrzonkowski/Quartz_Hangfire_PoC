using Serilog;

namespace Quartz.API.Extensions
{
    public static class ServiceCollectionQuartzConfiguratorExtensions
    {
        public static void AddJobAndTrigger<T>(
            this IServiceCollectionQuartzConfigurator quartz,
            IConfiguration config)
            where T : IJob
        {
            // Use the name of the IJob as the appsettings.json key
            string jobName = typeof(T).Name;

            // Try and load the schedule from configuration
            var configKey = $"Quartz:{jobName}CronSchedule";
            var cronSchedule = config[configKey];

            // Some minor validation
            if (string.IsNullOrWhiteSpace(cronSchedule))
            {
                throw new ArgumentNullException($"No Quartz.NET Cron schedule found for job in configuration at {configKey}");
            }

            // register the job
            var jobKey = new JobKey(jobName);
            quartz.AddJob<T>(opts => opts.WithIdentity(jobKey));

            quartz.AddTrigger(opts => opts
                .ForJob(jobKey)
                .WithIdentity(jobName + "-trigger")
                .WithCronSchedule(cronSchedule)); // use the schedule from configuration

            Log.Information($"Quartz background job '{jobName}' registered with Cron schedule: '{cronSchedule}'");
        }
    }
}