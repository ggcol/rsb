using Rsb.Core.Messaging;

namespace Rsb.Core.Enablers;

internal interface IHandlerBroker
{
    internal ICollectMessage Collector { get; }

    internal Task Handle(BinaryData binaryData,
        CancellationToken cancellationToken);

    internal Task HandleError(Exception ex,
        CancellationToken cancellationToken);
}

internal interface IHandlerBroker<T> : IHandlerBroker
{
}