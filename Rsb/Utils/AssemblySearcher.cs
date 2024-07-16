using System.Reflection;
using Type = System.Type;

namespace Rsb.Utils;

//TODO rename
public static class AssemblySearcher
{
    public static IReadOnlyList<Type> GetIHandleMessageImplementersTypes(Assembly? assembly)
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

        return listenersTypes is not null && listenersTypes.Any()
            ? listenersTypes
            : Array.Empty<Type>();
    }

    public static IEnumerable<Type> GetIHandleMessageImplementersMessageTypes(IReadOnlyList<Type> listenerTypes)
    {
        foreach (var listener in listenerTypes)
        {
            var implInterfaces = listener.GetInterfaces();

            foreach (var @interface in implInterfaces)
            {
                if (@interface.GetGenericTypeDefinition() ==
                    typeof(IHandleMessage<>))
                {
                    yield return @interface.GetGenericArguments().First();
                }
            }
        }
    }
    
    public static Type? GetIHandleMessageImplementerByMessageType(IReadOnlyList<Type> listenerTypes, Type messageType)
    {
        return listenerTypes.FirstOrDefault(l =>
            l.GetInterfaces().Any(i =>
                i.IsGenericType &&
                i.GetGenericTypeDefinition() == typeof(IHandleMessage<>) &&
                i.GetGenericArguments()[0] == messageType));
    }
}