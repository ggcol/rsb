namespace Rsb.Accessories.Heavy;

// ReSharper disable once InconsistentNaming
internal interface IHeavyIO
{
    internal Task<IReadOnlyList<HeavyRef>> Unload<TMessage>(
        TMessage message, Guid messageId,
        CancellationToken cancellationToken = default)
        where TMessage : IAmAMessage;

    internal Task Load(object message,
        IReadOnlyList<HeavyRef> heavies,
        Guid messageId,
        CancellationToken cancellationToken = default);
}