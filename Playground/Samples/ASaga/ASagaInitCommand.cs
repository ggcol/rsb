using Rsb;

namespace Playground.Samples.ASaga;

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