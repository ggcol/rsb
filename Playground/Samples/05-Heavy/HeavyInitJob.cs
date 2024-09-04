using Microsoft.Extensions.Hosting;
using Rsb;

namespace Playground.Samples._05_Heavy;

public class HeavyInitJob(
    IMessagingContext _context,
    IHostApplicationLifetime _hostApplicationLifetime) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await _context.Send(new HeavyCommand
            {
                AHeavyProp = new("Hello world!")
            }, cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _hostApplicationLifetime.StopApplication();
    }
}