namespace ASureBus.Core.Exceptions;

internal sealed class SagaNotFoundException : Exception
{
    internal SagaNotFoundException(Type sagaType, Guid correlationId)
        : base($"Saga of type {sagaType.Name} with correlation id {correlationId} not found")
    { }
}