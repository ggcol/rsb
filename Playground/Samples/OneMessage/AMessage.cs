using Rsb.Configurations;

namespace Playground.Samples.OneMessage;

public class AMessage : IAmACommand
{
    public string? Something { get; init; }
}