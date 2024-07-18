using Microsoft.Extensions.Hosting;
using Rsb;

namespace Playground;

public class AGenericJob : IHostedService
{
    private readonly IHostApplicationLifetime _hostApplicationLifetime;

    public AGenericJob(IHostApplicationLifetime hostApplicationLifetime)
    {
        _hostApplicationLifetime = hostApplicationLifetime;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var saga = new Prova();
        Console.WriteLine(saga.CorrelationId);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        Console.WriteLine("Bye!");
        _hostApplicationLifetime.StopApplication();
    }
}