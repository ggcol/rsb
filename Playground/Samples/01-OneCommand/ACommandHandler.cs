using ASureBus.Abstractions;
using Microsoft.Extensions.Logging;
using Playground.Samples._01_OneCommand.Messages;

namespace Playground.Samples._01_OneCommand;

public class ACommandHandler(ILogger<ACommandHandler> logger) : IHandleMessage<ACommand>
{
    public Task Handle(ACommand message, IMessagingContext context,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Handler starts");
        logger.LogInformation("{MessageSays}", message.Something);
        return Task.CompletedTask;
    }
}