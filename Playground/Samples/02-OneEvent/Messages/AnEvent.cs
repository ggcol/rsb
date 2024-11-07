using ASureBus.Abstractions;

namespace Playground.Samples._02_OneEvent.Messages;

public class AnEvent : IAmAnEvent
{
    public string? Something { get; init; }
}