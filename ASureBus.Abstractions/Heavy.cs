namespace ASureBus.Abstractions;

public abstract class Heavy
{
    internal Guid Ref = Guid.NewGuid();
}

public class Heavy<T>() : Heavy
{
    public T? Value { get; set; } = default;

    public Heavy(T value) : this()
    {
        Value = value;
    }
}