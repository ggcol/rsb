namespace ASureBus.Core.TypesHandling.Entities;

internal sealed class SagaHandlerType : ListenerType
{
    internal bool IsInitMessageHandler { get; init; }
}