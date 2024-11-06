using System.Reflection;
using ASureBus.Abstractions;
using ASureBus.Core.Caching;
using ASureBus.Core.TypesHandling.Entities;

namespace ASureBus.Core.Sagas;

internal sealed class SagaBehaviour(IAsbCache cache)
    : ISagaBehaviour
{
    private readonly ISagaIO? _sagaIo = RsbConfiguration.OffloadSagas
        ? new SagaIO()
        : null;

    public void SetCorrelationId(SagaType sagaType,
        Guid correlationId, object implSaga)
    {
        sagaType.Type
            .GetProperty(nameof(ISaga.CorrelationId))?
            .SetValue(implSaga, correlationId);
    }

    public void HandleCompletion(SagaType sagaType, Guid correlationId,
        object implSaga)
    {
        var completedEvent = sagaType.Type.GetEvent(nameof(ISaga.Completed));

        if (completedEvent == null) return;

        var handlerType = completedEvent.EventHandlerType;
        var methodInfo = GetType()
            .GetMethod(
                nameof(OnSagaCompleted),
                BindingFlags.NonPublic | BindingFlags.Instance);
        var handler = Delegate.CreateDelegate(handlerType, this, methodInfo);

        var addMethod = completedEvent.GetAddMethod(true);

        if (addMethod != null)
        {
            addMethod.Invoke(implSaga, new object[] { handler });
        }
    }

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