using System.Reflection;
using Rsb.Core.Caching;
using Rsb.Core.Sagas.Entities;
using Rsb.Core.TypesHandling.Entities;

namespace Rsb.Core.Sagas;

internal sealed class SagaBehaviour : ISagaBehaviour
{
    private readonly IRsbCache _cache;

    public SagaBehaviour(IRsbCache cache)
    {
        _cache = cache;
    }

    public void SetCorrelationId(SagaType sagaType,
        Guid correlationId, object implSaga)
    {
        sagaType.Type
            //TODO hardcoded string
            .GetProperty("CorrelationId",
                BindingFlags.Instance | BindingFlags.NonPublic)
            .SetValue(implSaga, correlationId);
    }

    public void HandleCompletion(SagaType sagaType, Guid correlationId,
        object implSaga)
    {
        var completedEvent = sagaType.Type
            //TODO hardcoded string 
            .GetEvent("Completed",
                BindingFlags.Instance | BindingFlags.NonPublic);

        if (completedEvent == null) return;
        
        var handlerType = completedEvent.EventHandlerType;
        var methodInfo = GetType()
            .GetMethod(
                nameof(OnSagaCompleted),
                BindingFlags.NonPublic | BindingFlags.Instance);
        var handler =
            Delegate.CreateDelegate(handlerType, this, methodInfo);

        var addMethod = completedEvent.GetAddMethod(true);
        if (addMethod != null)
        {
            addMethod.Invoke(implSaga,
                new object[] { handler }
            );
        }
    }

    private void OnSagaCompleted(object sender, SagaCompletedEventArgs e)
    {
        _cache.Remove(e.CorrelationId);
    }
}