namespace Rsb.Accessories.Heavy;

// ReSharper disable once InconsistentNaming
internal class UnusedHeavyIO : IHeavyIO
{
    public async Task<IReadOnlyList<HeavyRef>> Unload<TMessage>(TMessage message, Guid messageId,
        CancellationToken cancellationToken = default) where TMessage : IAmAMessage
    {
        return Array.Empty<HeavyRef>();
    }

    public Task Load(object message, IReadOnlyList<HeavyRef> heavies, Guid messageId,
        CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}