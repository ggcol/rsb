using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Playground;
using Playground.Samples.ASaga;
using Playground.Samples.OneCommand;
using Playground.Samples.OneEvent;
using Rsb.Core;

await Host
    .CreateDefaultBuilder()
    .UseRsb<Settings>()
    .ConfigureServices(
        (_, services) =>
        {
            // services.AddHostedService<OneCommandInitJob>();
            // services.AddHostedService<OneEventInitJob>();
            // services.AddHostedService<TwoMessagesSameHandlerClassInitJob>();
            services.AddHostedService<ASagaInitJob>();
            // services.AddHostedService<AGenericJob>();
            services.AddLogging();
        })
    .RunConsoleAsync();