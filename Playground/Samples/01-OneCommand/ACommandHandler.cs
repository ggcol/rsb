using Rsb;

namespace Playground.Samples._01_OneCommand;

public class ACommandHandler : IHandleMessage<ACommand>
{
    private const int FIVE_SECONDS = 5000;

    public async Task Handle(ACommand message, IMessagingContext context,
        CancellationToken cancellationToken = default)
    {
        Console.WriteLine("Handler starts");

        Thread.Sleep(FIVE_SECONDS);
        
        Console.WriteLine("Handler slept");

        Console.WriteLine(message.Something);
    }
}