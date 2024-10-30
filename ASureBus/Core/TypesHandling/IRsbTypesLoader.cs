using ASureBus.Core.TypesHandling.Entities;

namespace ASureBus.Core.TypesHandling;

internal interface IRsbTypesLoader
{
    internal HashSet<HandlerType> Handlers { get; }
    internal HashSet<SagaType> Sagas { get; }
}