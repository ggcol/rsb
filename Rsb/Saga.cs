namespace Rsb;

public abstract class Saga<T>
    where T : SagaData, new()
{
    public T SagaData { get; set; } = new();

    protected void IAmComplete()
    {
        SagaData.IsCompleted = true;
    }
}

public abstract class SagaData
{
    internal bool IsCompleted { get; set; }
}


public interface IAmStartedBy<in TMessage> : IHandleMessage<TMessage>
where TMessage : IAmAMessage
{ }
