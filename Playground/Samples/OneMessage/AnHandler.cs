using Rsb;

namespace Playground.Samples.OneMessage;

public class AnHandler : IHandleMessage<AMessage>
{
    public async Task Handle(AMessage message, IMessagingContext context,
        CancellationToken cancellationToken = default)
    {
        Console.WriteLine(message.Something);
    }
}