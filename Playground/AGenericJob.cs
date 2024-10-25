using Microsoft.Extensions.Hosting;

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
        //do sth
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        Console.WriteLine("Bye!");
        _hostApplicationLifetime.StopApplication();
    }
}