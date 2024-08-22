using Rsb.Core.TypesHandling.Entities;

namespace Rsb.Core.TypesHandling;

internal interface IRsbTypesLoader
{
    internal HashSet<HandlerType> Handlers { get; }
    internal HashSet<SagaType> Sagas { get; }
}