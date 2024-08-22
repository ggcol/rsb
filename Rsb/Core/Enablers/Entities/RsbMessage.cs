namespace Rsb.Core.Enablers.Entities;

internal sealed class RsbMessage<TMessage> : IRsbMessage
    where TMessage : IAmAMessage
{
    public Guid CorrelationId { get; init; }
    public TMessage Message { get; init; }
    public string MessageName { get; init; }
    public bool IsCommand => Message is IAmACommand;
}