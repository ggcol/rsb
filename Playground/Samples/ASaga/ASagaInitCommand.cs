using Rsb.Configurations;

namespace Playground.Samples.ASaga;

public class ASagaInitCommand : IAmACommand
{
}

public class AnotherCommand : IAmACommand
{
    public string Something { get; init; }   
}