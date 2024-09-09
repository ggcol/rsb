using Microsoft.Extensions.Hosting;
using Rsb;
using Rsb.Core.Messaging;

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
    
    public async Task StopAsync(CancellationToken cancellationToken)
    {
        hostApplicationLifetime.StopApplication();
    }
}