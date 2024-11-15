using ASureBus.Accessories.Heavy;

namespace ASureBus.Core.Entities;

internal interface IAsbMessage
{
    internal Guid MessageId { get; set; }
    internal Guid CorrelationId { get; init; }
    internal string MessageName { get; init; }
    internal string Destination { get; init; }
    internal bool IsCommand { get; }
    internal IReadOnlyList<HeavyReference>? Heavies { get; set; }
    internal bool IsScheduled { get; }
    internal DateTimeOffset? ScheduledTime { get; set; }
}