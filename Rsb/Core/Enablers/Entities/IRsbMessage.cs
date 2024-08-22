namespace Rsb.Core.Enablers.Entities;

internal interface IRsbMessage
{
    internal Guid CorrelationId { get; init; }
    internal string MessageName { get; init; }
    public bool IsCommand { get; }
}