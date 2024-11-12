using ASureBus.Abstractions;

namespace Playground.Samples._08_ABrokenSaga.Messages;

public class ABrokenSagaInitCommand : IAmACommand
{
    public bool BreakSaga { get; init; }
}