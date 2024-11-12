using System.Reflection;
using ASureBus.Abstractions;
using ASureBus.Core.Caching;
using ASureBus.Core.TypesHandling.Entities;

namespace ASureBus.Core.Sagas;

internal sealed class SagaBehaviour(IAsbCache cache)
    : ISagaBehaviour
{
    private readonly ISagaIO? _sagaIo = AsbConfiguration.OffloadSagas
        ? new SagaIO()
        : null;

    public void SetCorrelationId(SagaType sagaType, Guid correlationId, object sagaInstance)
    {
        sagaType.Type
            .GetProperty(nameof(ISaga.CorrelationId))?
            .SetValue(sagaInstance, correlationId);
    }

    public void HandleCompletion(SagaType sagaType, Guid correlationId, object sagaInstance)
    {
        var completedEvent = sagaType.Type.GetEvent(nameof(ISaga.Completed));

        if (completedEvent is null) return;

        var handlerType = completedEvent.EventHandlerType;
        
        var methodInfo = GetType().GetMethod(nameof(OnSagaCompleted), BindingFlags.NonPublic | BindingFlags.Instance);
        
        var handler = Delegate.CreateDelegate(handlerType, this, methodInfo);

        var addMethod = completedEvent.GetAddMethod(true);

        if (addMethod != null)
        {
            addMethod.Invoke(sagaInstance, new object[] { handler });
        }
    }

    // ReSharper disable once UnusedParameter.Local
    // this is used through reflection
    private void OnSagaCompleted(object sender, SagaCompletedEventArgs e)
    {
        cache.Remove(e.CorrelationId);
        if (_sagaIo is not null)
        {
            _sagaIo.Delete(e.CorrelationId, new SagaType
                {
                    Type = e.Type
                })
                .ConfigureAwait(false).GetAwaiter();
        }
    }
}