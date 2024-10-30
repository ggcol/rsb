using System.Reflection;
using ASureBus.Abstractions;
using ASureBus.Core;
using ASureBus.Services.StorageAccount;

namespace ASureBus.Accessories.Heavy;

// ReSharper disable once InconsistentNaming
internal sealed class HeavyIO(IAzureDataStorageService storage)
    : IHeavyIO
{
    public async Task<IReadOnlyList<HeavyRef>> Unload<TMessage>(
        TMessage message, Guid messageId,
        CancellationToken cancellationToken = default)
        where TMessage : IAmAMessage
    {
        var heavyProps = message
            .GetType()
            .GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .Where(prop =>
                prop.PropertyType.IsGenericType &&
                prop.PropertyType.GetGenericTypeDefinition() ==
                typeof(Heavy<>))
            .ToArray();

        var heaviesRef = new List<HeavyRef>();
        if (heavyProps.Length == 0) return heaviesRef;

        foreach (var heavyProp in heavyProps)
        {
            var value = heavyProp.GetValue(message);
            var heavyRef = (value as Heavy).Ref;

            await storage.Save(value,
                    RsbConfiguration.HeavyProps?.DataStorageContainer!,
                    string.Join('-', messageId, heavyRef),
                    false,
                    cancellationToken)
                .ConfigureAwait(false);

            heavyProp.SetValue(message, null);

            heaviesRef.Add(new HeavyRef
            {
                PropName = heavyProp.Name,
                Ref = heavyRef
            });
        }

        return heaviesRef;
    }

    public async Task Load(object message,
        IReadOnlyList<HeavyRef> heavies,
        Guid messageId,
        CancellationToken cancellationToken = default)
    {
        foreach (var heavyRef in heavies)
        {
            var prop = message
                .GetType()
                .GetProperties()
                .FirstOrDefault(prop =>
                    prop.Name.Equals(heavyRef.PropName));

            var propType = prop.PropertyType.GetGenericArguments()
                .First();

            var heavyGenericType = typeof(Heavy<>).MakeGenericType(propType);

            var blobName = string.Join('-', messageId, heavyRef.Ref);
            var value = await storage.Get(
                    RsbConfiguration.HeavyProps?.DataStorageContainer!,
                    blobName,
                    heavyGenericType,
                    cancellationToken: cancellationToken)
                .ConfigureAwait(false);

            prop.SetValue(message, value);

            await storage.Delete(
                    RsbConfiguration.HeavyProps?.DataStorageContainer!,
                    blobName,
                    cancellationToken)
                .ConfigureAwait(false);
        }
    }
}