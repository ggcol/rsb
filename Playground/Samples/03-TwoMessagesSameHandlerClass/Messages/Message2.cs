using ASureBus.Abstractions;

namespace Playground.Samples._03_TwoMessagesSameHandlerClass.Messages;

public record Message2(string Something) : IAmACommand{}