using Rsb.Configurations;

namespace Rsb;

public abstract class NewSaga<T>
    where T : SagaData
{
    public T SagaData { get; set; }

    protected void IAmComplete()
    {
        SagaData.IsCompleted = true;
    }
}

public abstract class SagaData
{
    public Guid CorrelationId { get; init; } = Guid.NewGuid();
    internal bool IsCompleted { get; set; }
}


public interface IAmStartedBy<in TMessage> : IHandleMessage<TMessage>
where TMessage : IAmAMessage
{ }
