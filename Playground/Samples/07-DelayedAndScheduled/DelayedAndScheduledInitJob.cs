using ASureBus.Abstractions;
using Microsoft.Extensions.Hosting;

namespace Playground.Samples._07_DelayedAndScheduled;

public class DelayedAndScheduledInitJob(
    IMessagingContext context,
    IHostApplicationLifetime hostApplicationLifetime) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var delay = TimeSpan.FromSeconds(20);
        var message = new DelayedMessage()
        {
            CreatedAt = DateTimeOffset.UtcNow,
            Delay = delay
        };

        await context.SendAfter(message, delay, cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        hostApplicationLifetime.StopApplication();
    }
}