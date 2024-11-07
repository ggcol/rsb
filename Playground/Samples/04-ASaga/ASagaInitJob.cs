using ASureBus.Abstractions;
using Microsoft.Extensions.Hosting;
using Playground.Samples._04_ASaga.Messages;

namespace Playground.Samples._04_ASaga;

internal class ASagaInitJob(
    IMessagingContext context,
    IHostApplicationLifetime hostApplicationLifetime)
    : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await context.Send(new ASagaInitCommand(), cancellationToken)
            .ConfigureAwait(false);
    }
    
    public Task StopAsync(CancellationToken cancellationToken)
    {
        hostApplicationLifetime.StopApplication();
        return Task.CompletedTask;
    }
}