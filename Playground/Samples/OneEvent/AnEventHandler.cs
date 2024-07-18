using Rsb;

namespace Playground.Samples.OneEvent;

public class AnEventHandler : IHandleMessage<AnEvent>
{
    private const int FIVE_SECONDS = 5000;

    public async Task Handle(AnEvent message, IMessagingContext context,
        CancellationToken cancellationToken = default)
    {
        Console.WriteLine("Handler starts");

        Thread.Sleep(FIVE_SECONDS);

        Console.WriteLine("Handler slept");

        Console.WriteLine(message.Something);
    }
}