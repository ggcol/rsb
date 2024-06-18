using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using rsb.Configurations;
using rsb.HostBuilders.Utils;
using rsb.Services;
using Serilog;

namespace rsb.HostBuilders;

public static class SchedulerHost
{
    public static IHostBuilder Create<TSettings, TJob>()
        where TSettings : class, IConfigureAzureServiceBus, IConfigureSchedulerJob, new()
        where TJob : SchedulingUtils.SchedulerJob<TSettings>
    {
        return new HostBuilder()
            .ConfigureServices((hostBuilderContext, services) =>
            {
                //TODO
                services.Configure<TSettings>(hostBuilderContext.Configuration.GetSection("TODO"));

                services.AddMemoryCache();

                services
                    .AddScoped<IAzureServiceBusService, AzureServiceBusService<TSettings>>()
                    .AddScoped<IMessagingContext, MessagingContext>()
                    .AddHostedService<TJob>();
            })
            .UseSerilog(ConfigureHost.Logger);
    }
}