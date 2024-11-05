using ASureBus.Abstractions;
using ASureBus.Accessories.Heavy;

namespace ASureBus.Core.Entities;

internal class RsbMessage<TMessage> : IRsbMessage
    where TMessage : IAmAMessage
{
    public Guid MessageId { get; set; }
    public Guid CorrelationId { get; init; }
    public TMessage Message { get; init; }
    public string MessageName { get; init; }
    public bool IsCommand => Message is IAmACommand;
    public IReadOnlyList<HeavyRef>? Heavies { get; init; }
    public bool IsScheduled => ScheduledTime.HasValue;
    public DateTimeOffset? ScheduledTime { get; init; }
}