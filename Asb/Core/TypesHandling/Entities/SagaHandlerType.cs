namespace Asb.Core.TypesHandling.Entities;

internal sealed class SagaHandlerType : ListenerType
{
    internal bool IsInitMessage { get; init; }
}