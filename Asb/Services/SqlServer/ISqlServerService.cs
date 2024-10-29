using Asb.Core.TypesHandling.Entities;

namespace Asb.Services.SqlServer;

internal interface ISqlServerService
{
    internal Task Save<TItem>(TItem item, SagaType sagaType,
        Guid correlationId, CancellationToken cancellationToken = default);

    internal Task<object?> Get(SagaType sagaType, Guid correlationId,
        CancellationToken cancellationToken = default);

    internal Task Delete(SagaType sagaType, Guid correlationId,
        CancellationToken cancellationToken = default);
}