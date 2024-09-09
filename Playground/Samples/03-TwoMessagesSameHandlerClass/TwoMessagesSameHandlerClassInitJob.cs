using Microsoft.Extensions.Hosting;
using Rsb;

namespace Playground.Samples._03_TwoMessagesSameHandlerClass;

internal class TwoMessagesSameHandlerClassInitJob(
    IMessagingContext context,
    IHostApplicationLifetime hostApplicationLifetime)
    : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await context.Send(new Message1(), cancellationToken)
            .ConfigureAwait(false);
        
        await context.Send(new Message2(), cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        hostApplicationLifetime.StopApplication();
    }
}