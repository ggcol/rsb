using ASureBus.Abstractions;
using Microsoft.Extensions.Hosting;
using Playground.Samples._08_ABrokenSaga.Messages;

namespace Playground.Samples._08_ABrokenSaga;

public class ABrokenSagaInitJob(
    IHostApplicationLifetime applicationLifetime,
    IMessagingContext context) 
    : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await context.Send(new ABrokenSagaInitCommand()
            {
                BreakSaga = true
            }, cancellationToken)
            .ConfigureAwait(false);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        applicationLifetime.StopApplication();
        return Task.CompletedTask;
    }
}