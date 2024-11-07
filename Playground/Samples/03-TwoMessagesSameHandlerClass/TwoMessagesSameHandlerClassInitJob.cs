using ASureBus.Abstractions;
using Microsoft.Extensions.Hosting;
using Playground.Samples._03_TwoMessagesSameHandlerClass.Messages;

namespace Playground.Samples._03_TwoMessagesSameHandlerClass;

internal class TwoMessagesSameHandlerClassInitJob(
    IMessagingContext context,
    IHostApplicationLifetime hostApplicationLifetime)
    : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await context.Send(new Message1("Hello!"), cancellationToken)
            .ConfigureAwait(false);

        await context.Send(new Message2("World!"), cancellationToken)
            .ConfigureAwait(false);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        hostApplicationLifetime.StopApplication();
        return Task.CompletedTask;
    }
}