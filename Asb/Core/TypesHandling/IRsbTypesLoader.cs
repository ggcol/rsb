using Asb.Core.TypesHandling.Entities;

namespace Asb.Core.TypesHandling;

internal interface IRsbTypesLoader
{
    internal HashSet<HandlerType> Handlers { get; }
    internal HashSet<SagaType> Sagas { get; }
}