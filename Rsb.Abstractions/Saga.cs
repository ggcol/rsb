namespace Rsb.Abstractions;

public abstract class Saga<T>
    where T : SagaData, new()
{
    public T SagaData { get; internal set; } = new();

    public Guid CorrelationId { get; set; }
    internal event EventHandler<SagaCompletedEventArgs>? Completed;

    // ReSharper disable once InconsistentNaming
    protected void IAmComplete()
    {
        Completed?.Invoke(this, new ()
        {
            CorrelationId = CorrelationId,
            Type = GetType()
        });
    }
}

internal sealed class SagaCompletedEventArgs : EventArgs
{
    internal Guid CorrelationId { get; init; }
    internal Type? Type { get; init; }
}
