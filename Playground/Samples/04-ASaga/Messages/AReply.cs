using ASureBus.Abstractions;

namespace Playground.Samples._04_ASaga.Messages;

public class AReply : IAmACommand
{
    public string? Something { get; init; }
}