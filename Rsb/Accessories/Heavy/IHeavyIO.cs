namespace Rsb.Accessories.Heavy;

// ReSharper disable once InconsistentNaming
internal interface IHeavyIO
{
    public Task<IReadOnlyList<HeavyRef>> Unload<TMessage>(
        TMessage message, Guid messageId,
        CancellationToken cancellationToken = default)
        where TMessage : IAmAMessage;

    public Task Load(object message,
        IReadOnlyList<HeavyRef> heavies,
        Guid messageId,
        CancellationToken cancellationToken = default);
}