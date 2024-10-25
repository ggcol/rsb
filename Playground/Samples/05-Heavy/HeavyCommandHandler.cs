using Rsb.Abstractions;

namespace Playground.Samples._05_Heavy;

public class HeavyCommandHandler : IHandleMessage<HeavyCommand>
{
    public async Task Handle(HeavyCommand message, IMessagingContext context,
        CancellationToken cancellationToken = default)
    {
        Console.WriteLine(message.AHeavyProp.Value);
    }
}