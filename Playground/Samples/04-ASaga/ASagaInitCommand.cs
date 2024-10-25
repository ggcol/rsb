using Rsb.Abstractions;

namespace Playground.Samples._04_ASaga;

public class ASagaInitCommand : IAmACommand
{
}

public class AReply : IAmACommand
{
    public string Something { get; init; }
}

public class AnotherCommand : IAmACommand
{
    public string Something { get; init; }   
}