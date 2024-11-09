using ASureBus.Core.Entities;
using ASureBus.Core.Messaging;

namespace ASureBus.Core.Enablers;

internal interface IHandlerBroker
{
    internal ICollectMessage Collector { get; }

    internal Task<IAsbMessage> Handle(BinaryData binaryData,
        CancellationToken cancellationToken);

    internal Task HandleError(Exception ex,
        CancellationToken cancellationToken);
}

//TODO check this
// ReSharper disable once UnusedTypeParameter
internal interface IHandlerBroker<T> : IHandlerBroker
{
}