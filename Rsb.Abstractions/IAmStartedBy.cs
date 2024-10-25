namespace Rsb.Abstractions;

public interface IAmStartedBy<in TMessage> : IHandleMessage<TMessage>
    where TMessage : IAmAMessage
{ }