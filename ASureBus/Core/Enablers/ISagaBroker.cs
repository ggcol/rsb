using ASureBus.Core.Entities;
using ASureBus.Core.Messaging;

namespace ASureBus.Core.Enablers;

internal interface ISagaBroker
{
    internal ICollectMessage Collector { get; }

    internal Task<IAsbMessage> Handle(BinaryData binaryData,
        CancellationToken cancellationToken = default);

    internal Task HandleError(Exception ex,
        CancellationToken cancellationToken = default);
}