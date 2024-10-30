using ASureBus.Abstractions;

namespace Playground.Samples._06_SagaPersistence;

public class APersistedSagaReply(string something) : IAmACommand
{
    public string Something { get; set; } = something;
}