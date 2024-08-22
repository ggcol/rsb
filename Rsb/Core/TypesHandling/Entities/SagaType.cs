namespace Rsb.Core.TypesHandling.Entities;

internal sealed class SagaType
{
    internal Type Type { get; init; }
    internal Type SagaDataType { get; init; }
    internal HashSet<SagaHandlerType> Listeners { get; set; }
}