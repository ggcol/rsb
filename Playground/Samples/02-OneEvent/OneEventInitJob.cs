using ASureBus.Abstractions;
using Microsoft.Extensions.Hosting;
using Playground.Samples._02_OneEvent.Messages;

namespace Playground.Samples._02_OneEvent;

internal class OneEventInitJob(
    IMessagingContext context,
    IHostApplicationLifetime hostApplicationLifetime)
    : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var max = new Random().Next(1, 5);

        for (var i = 0; i <= max; i++)
        {
            await context.Publish(new AnEvent
                {
                    Something = $"{i} - Hello world!"
                }, cancellationToken)
                .ConfigureAwait(false);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        hostApplicationLifetime.StopApplication();
        return Task.CompletedTask;
    }
}