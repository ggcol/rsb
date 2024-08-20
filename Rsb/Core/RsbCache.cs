using System.Reflection;
using Rsb.Configurations;

namespace Rsb.Core;

internal interface IRsbCache
{
    internal HashSet<ListenerType> Listeners { get; }
    internal HashSet<MessageType> Messages { get; }
}

internal class ListenerType
{
    internal Type Type { get; init; }
    internal MessageType MessageType { get; init; }
    internal bool IsInSaga { get; init; }
}

internal class MessageType
{
    internal Type Type { get; init; }
    internal bool IsCommand { get; init; }
}

internal class RsbCache : IRsbCache
{
    public HashSet<ListenerType> Listeners { get; }

    public HashSet<MessageType> Messages =>
        Listeners.Select(x => x.MessageType).ToHashSet();

    public RsbCache()
    {
        Listeners =
            GetIHandleMessageImplementersTypes(Assembly.GetEntryAssembly())
                .ToHashSet();
    }

    private IEnumerable<ListenerType> GetIHandleMessageImplementersTypes(
        Assembly? assembly)
    {
        var listenersTypes = assembly?
            .GetTypes()
            .Where(t => t is { IsClass: true, IsAbstract: false })
            .Where(t =>
                t.GetInterfaces().Any(i =>
                    i.IsGenericType && i.GetGenericTypeDefinition() ==
                    typeof(IHandleMessage<>))
            )
            .ToArray();

        if (listenersTypes is null || !listenersTypes.Any()) yield break;

        foreach (var listener in listenersTypes)
        {
            var isInSaga = listener.BaseType != null &&
                           listener.BaseType != typeof(object) &&
                           listener.BaseType.GetGenericTypeDefinition() ==
                           typeof(NewSaga<>);

            var implInterfaces = listener.GetInterfaces();

            foreach (var @interface in implInterfaces)
            {
                if (@interface.GetGenericTypeDefinition() !=
                    typeof(IHandleMessage<>)) continue;
                
                var messageType = @interface.GetGenericArguments().First();
                var isCommand = messageType.GetInterfaces()
                    .Any(x => x == typeof(IAmACommand));
                    
                yield return new ListenerType()
                {
                    Type = listener,
                    MessageType = new MessageType()
                    {
                        Type = messageType,
                        IsCommand = isCommand
                    },
                    IsInSaga = isInSaga
                };
            }
        }
    }
}