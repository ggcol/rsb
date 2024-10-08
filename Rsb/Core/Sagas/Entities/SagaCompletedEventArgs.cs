﻿namespace Rsb.Core.Sagas.Entities;

internal sealed class SagaCompletedEventArgs : EventArgs
{
    internal Guid CorrelationId { get; init; }
    internal Type Type { get; set; }
}