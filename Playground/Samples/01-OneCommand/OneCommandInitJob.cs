using Microsoft.Extensions.Hosting;
using Rsb;

namespace Playground.Samples._01_OneCommand;

internal class OneCommandInitJob : IHostedService
{
    private readonly IMessagingContext _context;
    private readonly IHostApplicationLifetime _hostApplicationLifetime;

    public OneCommandInitJob(IMessagingContext context,
        IHostApplicationLifetime hostApplicationLifetime)
    {
        _context = context;
        _hostApplicationLifetime = hostApplicationLifetime;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var max = new Random().Next(5);

        for (var i = 0; i <= max; i++)
        {
            await _context.Send(new ACommand
                {
                    Something = $"{i} - Hello world!"
                }, cancellationToken)
                .ConfigureAwait(false);
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _hostApplicationLifetime.StopApplication();
    }
}