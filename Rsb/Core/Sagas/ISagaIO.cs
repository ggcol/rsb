using Rsb.Core.TypesHandling.Entities;

namespace Rsb.Core.Sagas;

// ReSharper disable once InconsistentNaming
internal interface ISagaIO
{
    public Task<object?> Load(Guid correlationId, SagaType sagaType,
        CancellationToken cancellationToken = default);

    public Task Unload(object? implSaga, Guid correlationId,
        CancellationToken cancellationToken = default);

    public Task Delete(Guid correlationId,
        CancellationToken cancellationToken = default);
}