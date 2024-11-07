using ASureBus.Abstractions;

namespace Playground.Samples._01_OneCommand.Messages;

public class ACommand : IAmACommand
{
    public string? Something { get; init; }
}