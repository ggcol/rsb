namespace Rsb.Core.TypesHandling.Entities;

internal class MessageType
{
    internal Type Type { get; init; }
    internal bool IsCommand { get; init; }
}