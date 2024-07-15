using System.Reflection;

namespace Rsb.Utils;

public static class AssemblySearcher
{
    public static IEnumerable<Type>? GetListeners()
    {
        return Assembly.GetEntryAssembly()?
            .GetTypes()
            .Where(t => t is { IsClass: true, IsAbstract: false })
            .Where(t =>
                t.GetInterfaces().Any(i =>
                    i.IsGenericType && i.GetGenericTypeDefinition() ==
                    typeof(IHandleMessage<>))
            );
    }
}