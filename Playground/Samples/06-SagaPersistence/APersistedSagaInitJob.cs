using Microsoft.Extensions.Hosting;
using Rsb.Abstractions;

namespace Playground.Samples._06_SagaPersistence;

public class APersistedSagaInitJob(
    IHostApplicationLifetime applicationLifetime,
    IMessagingContext context)
    : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await context.Send(new APersistedSagaInitCommand(), cancellationToken)
            .ConfigureAwait(false);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        applicationLifetime.StopApplication();
        return Task.CompletedTask;
    }
}