using Rsb.Abstractions;

namespace Playground.Samples._01_OneCommand;

public class ACommand : IAmACommand
{
    public string? Something { get; init; }
}