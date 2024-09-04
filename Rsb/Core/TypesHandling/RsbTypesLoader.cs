using System.Reflection;
using Rsb.Core.TypesHandling.Entities;

namespace Rsb.Core.TypesHandling;

internal sealed class RsbTypesLoader : IRsbTypesLoader
{
    public HashSet<HandlerType> Handlers { get; }
    public HashSet<SagaType> Sagas { get; }

    private HashSet<Type> _sagaMessages => Sagas
        .SelectMany(s => s.Listeners)
        .Select(sagaListener => sagaListener.MessageType.Type)
        .ToHashSet();

    private HashSet<Type> _handlersMessages => Handlers
        .Select(x => x.MessageType.Type)
        .ToHashSet();

    public RsbTypesLoader()
    {
        var assembly = Assembly.GetEntryAssembly();
        Sagas = GetSagas(assembly).ToHashSet();
        Handlers = GetHandlers(assembly).ToHashSet();
    }

    private IEnumerable<HandlerType> GetHandlers(
        Assembly? assembly)
    {
        var handlersTypes = assembly?
            .GetTypes()
            .Where(t => t is { IsClass: true, IsAbstract: false })
            .Where(t =>
                t.GetInterfaces()
                    .Any(i =>
                        i.IsGenericType && i.GetGenericTypeDefinition() ==
                        typeof(IHandleMessage<>))
            )
            //exclude what's already under a saga scope
            .Where(t =>
                !t.GetInterfaces()
                    .Where(i =>
                        i.IsGenericType && i.GetGenericTypeDefinition() ==
                        typeof(IHandleMessage<>))
                    .Select(i => i.GetGenericArguments().First())
                    .Any(messageType =>
                        _sagaMessages.Contains(messageType)))
            .ToArray();

        if (handlersTypes is null || !handlersTypes.Any())
            return Array.Empty<HandlerType>();

        return GetListeners(handlersTypes);
    }

    private IEnumerable<SagaType> GetSagas(Assembly? assembly)
    {
        var sagas = assembly?
            .GetTypes()
            .Where(t => t is { IsClass: true, IsAbstract: false })
            .Where(t =>
                t.BaseType != null &&
                t.BaseType != typeof(object) &&
                t.BaseType.IsGenericType &&
                t.BaseType.GetGenericTypeDefinition() ==
                typeof(Saga<>))
            .ToArray();

        if (sagas is null || !sagas.Any()) yield break;

        foreach (var saga in sagas)
        {
            var sagaType = new SagaType
            {
                Type = saga,
                SagaDataType = saga.BaseType.GetGenericArguments().First(),
                Listeners = new HashSet<SagaHandlerType>()
            };

            var interfaces = saga
                .GetInterfaces()
                .Where(i =>
                    i.IsGenericType &&
                    i.GetGenericTypeDefinition() ==
                    typeof(IHandleMessage<>));

            foreach (var i in interfaces)
            {
                var messageType = i.GetGenericArguments().First();
                sagaType.Listeners.Add(new SagaHandlerType
                {
                    MessageType = new MessageType
                    {
                        Type = messageType,
                        IsCommand = messageType.GetInterfaces()
                            .Any(x => x == typeof(IAmACommand))
                    }
                });
            }

            yield return sagaType;
        }
    }

    private IEnumerable<HandlerType> GetListeners(
        IReadOnlyList<Type> handlerTypes)
    {
        foreach (var handler in handlerTypes)
        {
            var implInterfaces = handler.GetInterfaces();

            foreach (var @interface in implInterfaces)
            {
                if (@interface.GetGenericTypeDefinition() !=
                    typeof(IHandleMessage<>)) continue;

                var messageType = @interface.GetGenericArguments().First();
                var isCommand = messageType.GetInterfaces()
                    .Any(x => x == typeof(IAmACommand));

                yield return new HandlerType
                {
                    Type = handler,
                    MessageType = new MessageType
                    {
                        Type = messageType,
                        IsCommand = isCommand
                    }
                };
            }
        }
    }
}