using Rsb;

namespace Playground.Samples.OneMessage;

public class AnHandler : IHandleMessage<AMessage>
{
    private const int FIVE_SECONDS = 5000;

    public async Task Handle(AMessage message, IMessagingContext context,
        CancellationToken cancellationToken = default)
    {
        Console.WriteLine("Handler starts");

        Thread.Sleep(FIVE_SECONDS);
        
        Console.WriteLine("Handler slept");

        Console.WriteLine(message.Something);
    }
}