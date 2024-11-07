using ASureBus.Abstractions;

namespace Playground.Samples._07_DelayedAndScheduled.Messages;

public class ScheduledMessage : IAmACommand
{
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset ScheduledAt { get; init; }
}