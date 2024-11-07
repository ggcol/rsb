using ASureBus.Abstractions;
using Microsoft.Extensions.Logging;
using Playground.Samples._05_Heavy.Messages;

namespace Playground.Samples._05_Heavy;

public class HeavyCommandHandler(ILogger<HeavyCommandHandler> logger) : IHandleMessage<HeavyCommand>
{
    public Task Handle(HeavyCommand message, IMessagingContext context,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("{MessageSays}", message.AHeavyProp?.Value);
        return Task.CompletedTask;
    }
}