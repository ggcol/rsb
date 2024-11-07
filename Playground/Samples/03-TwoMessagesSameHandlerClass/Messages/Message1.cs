using ASureBus.Abstractions;

namespace Playground.Samples._03_TwoMessagesSameHandlerClass.Messages;

public record Message1(string Something) : IAmACommand{}