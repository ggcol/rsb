using Rsb.Services;

namespace Rsb.Workers;

internal interface IListenerBroker
{
    internal ICollectMessage? Collector { get; }

    internal Task Handle(BinaryData binaryData,
        CancellationToken cancellationToken);

    internal Task HandleError(Exception ex,
        CancellationToken cancellationToken);
}

internal interface IListenerBroker<T> : IListenerBroker
{
}