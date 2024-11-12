namespace ASureBus.Core.Exceptions;

internal sealed class AsbException : Exception
{
    internal required Exception OriginalException { get; init; }
    internal required Guid CorrelationId { get; init; }
}