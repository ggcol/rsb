namespace ASureBus.Abstractions.Options.Messaging;

public abstract class EmitOptions
{
    public string Destination { get; set; } = string.Empty;
    public bool IsScheduled => ScheduledTime is not null;
    public DateTimeOffset? ScheduledTime { get; set; }
    public TimeSpan? Delay
    {
        get => ScheduledTime - DateTimeOffset.UtcNow;
        set => ScheduledTime = DateTimeOffset.UtcNow.Add(value ?? TimeSpan.Zero);
    }
}