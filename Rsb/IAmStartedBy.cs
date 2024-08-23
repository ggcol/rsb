namespace Rsb;

public interface IAmStartedBy<in TMessage> : IHandleMessage<TMessage>
    where TMessage : IAmAMessage
{ }