
// ReSharper disable InconsistentNaming

namespace Rsb.Configurations;

internal static class RsbConfiguration
{
    public static ServiceBusConfig ServiceBus { get; set; } = new();
    public static RsbCacheConfig Cache { get; set; } = new();
    internal static bool UseHeavyProperties => HeavyProps is not null;
    public static HeavyPropertiesConfig? HeavyProps { get; set; }
}