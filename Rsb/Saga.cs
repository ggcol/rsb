using Rsb.Core.Sagas.Entities;

namespace Rsb;

public abstract class Saga<T>
    where T : SagaData, new()
{
    public T SagaData { get; internal set; } = new();

    internal Guid CorrelationId { get; set; }
    internal event EventHandler<SagaCompletedEventArgs>? Completed;

    protected void IAmComplete()
    {
        Completed?.Invoke(this, new ()
        {
            CorrelationId = CorrelationId
        });
    }
}
