﻿using ASureBus.Core.TypesHandling.Entities;

namespace ASureBus.Core.Sagas;

internal interface ISagaBehaviour
{
    internal void SetCorrelationId(SagaType sagaType,
        Guid correlationId, object implSaga);

    internal void HandleCompletion(SagaType sagaType, Guid correlationId,
        object implSaga);
}