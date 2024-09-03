using System.Reflection;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Rsb.Configurations;
using Rsb.Services.StorageAccount;

namespace Rsb.Accessories.Heavy;

// ReSharper disable once InconsistentNaming
internal class HeavyIO<TSettings>(
    IOptions<TSettings> options,
    IAzureBlobStorageService storage)
    : IHeavyIO
    where TSettings : class, IUseHeavyProperties, new()
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
                    options.Value.DataStorageContainer,
                    string.Join('-', messageId, heavyRef),
                    false,
                    cancellationToken)
                .ConfigureAwait(false);

            heavyProp.SetValue(message, null);

            heaviesRef.Add(new HeavyRef()
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
            var blobName = string.Join('-', messageId, heavyRef.Ref);
            var read = await storage.Get(
                    options.Value.DataStorageContainer,
                    blobName,
                    cancellationToken)
                .ConfigureAwait(false);

            var prop = message
                .GetType()
                .GetProperties()
                .FirstOrDefault(prop =>
                    prop.Name.Equals(heavyRef.PropName));

            var propType = prop.PropertyType.GetGenericArguments()
                .First();

            var heavyGeneric = typeof(Heavy<>).MakeGenericType(propType);

            var value = JsonSerializer.Deserialize(read, heavyGeneric);

            prop.SetValue(message, value);

            await storage.Delete(options.Value.DataStorageContainer, blobName,
                    cancellationToken)
                .ConfigureAwait(false);
        }
    }
}