namespace Asb.Core.Messaging;

internal interface IMessageEmitter
{
    internal Task FlushAll(ICollectMessage collector,
        CancellationToken cancellationToken = default);
}