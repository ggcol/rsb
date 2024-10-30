using ASureBus.Core.Messaging;

namespace ASureBus.Core.Enablers;

internal interface ISagaBroker
{
    internal ICollectMessage Collector { get; }

    public Task Handle(BinaryData binaryData,
        CancellationToken cancellationToken = default);

    public Task HandleError(Exception ex,
        CancellationToken cancellationToken = default);
}