using ASureBus.Abstractions;
using ASureBus.Accessories.Heavy;

namespace ASureBus.Core.Entities;

internal class AsbMessage<TMessage> : IAsbMessage
    where TMessage : IAmAMessage
{
    public required Guid MessageId { get; set; }
    public required Guid CorrelationId { get; init; }
    public required TMessage Message { get; init; }
    public required string MessageName { get; init; }
    public required string Destination { get; init; }
    public bool IsCommand => Message is IAmACommand;
    public IReadOnlyList<HeavyReference>? Heavies { get; set; }
    public bool IsScheduled => ScheduledTime.HasValue;
    public DateTimeOffset? ScheduledTime { get; set; }
}