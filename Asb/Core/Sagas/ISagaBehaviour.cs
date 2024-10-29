using Asb.Core.TypesHandling.Entities;

namespace Asb.Core.Sagas;

internal interface ISagaBehaviour
{
    internal void SetCorrelationId(SagaType sagaType,
        Guid correlationId, object implSaga);

    internal void HandleCompletion(SagaType sagaType, Guid correlationId,
        object implSaga);
}