using ASureBus.Core.TypesHandling.Entities;

namespace ASureBus.Core.TypesHandling;

internal interface ITypesLoader
{
    internal HashSet<HandlerType> Handlers { get; }
    internal HashSet<SagaType> Sagas { get; }
}