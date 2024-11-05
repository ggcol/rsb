using ASureBus.Abstractions;
using Microsoft.Extensions.Logging;

namespace Playground.Samples._07_DelayedAndScheduled;

public class ScheduledMessageHandler(ILogger<ScheduledMessageHandler> logger)
    : IHandleMessage<ScheduledMessage>
{
    public async Task Handle(ScheduledMessage message, IMessagingContext context,
        CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;
        logger.LogInformation("""
                              Scheduled message created at {0} 
                              with scheduled time: {1} 
                              received at {2}
                              {3}
                              """,
            message.CreatedAt, message.ScheduledAt, now, message.ScheduledAt <= now);
    }
}