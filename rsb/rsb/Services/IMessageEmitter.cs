namespace rsb.Services;

internal interface IMessageEmitter
{
    internal Task FlushAll(ICollectMessage collector,
        CancellationToken cancellationToken = default);
}