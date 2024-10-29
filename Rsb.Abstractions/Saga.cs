namespace Rsb.Abstractions;

internal interface ISaga
{
    public Guid CorrelationId { get; set; }
    internal event EventHandler<SagaCompletedEventArgs>? Completed;
}

public abstract class Saga<T> : ISaga
    where T : SagaData, new()
{
    public T SagaData { get; internal set; } = new();

    public Guid CorrelationId { get; set; }
    public event EventHandler<SagaCompletedEventArgs>? Completed;

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

public sealed class SagaCompletedEventArgs : EventArgs
{
    internal Guid CorrelationId { get; init; }
    internal Type? Type { get; init; }
}
