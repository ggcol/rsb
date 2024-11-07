using ASureBus.Abstractions;

namespace Playground.Samples._07_DelayedAndScheduled.Messages;

public class DelayedMessage : IAmACommand
{
    public DateTimeOffset CreatedAt { get; init; }
    public TimeSpan Delay { get; init; }
}