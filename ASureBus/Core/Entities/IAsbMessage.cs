using ASureBus.Accessories.Heavy;

namespace ASureBus.Core.Entities;

internal interface IAsbMessage
{
    internal Guid MessageId { get; set; }
    internal Guid CorrelationId { get; init; }
    internal string MessageName { get; init; }
    public bool IsCommand { get; }
    public IReadOnlyList<HeavyReference>? Heavies { get; init; }
    public bool IsScheduled { get; }
    public DateTimeOffset? ScheduledTime { get; init; }
}