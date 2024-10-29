// ReSharper disable InconsistentNaming

using Rsb.Configurations.ConfigObjects;

namespace Rsb.Core;

internal static class RsbConfiguration
{
    //basic configuration
    public static ServiceBusConfig ServiceBus { get; set; } = new();
    public static RsbCacheConfig Cache { get; set; } = new();

    //heavy props configuration
    internal static bool UseHeavyProperties => HeavyProps is not null;
    public static HeavyPropertiesConfig? HeavyProps { get; set; }

    //saga offloading configuration
    internal static bool OffloadSagas =>
        UseDataStorageSagaPersistence || UseSqlServerSagaPersistence;
    internal static bool UseDataStorageSagaPersistence => DataStorageSagaPersistence is not null;
    internal static bool UseSqlServerSagaPersistence => SqlServerSagaPersistence is not null;
    public static DataStorageSagaPersistenceConfig? DataStorageSagaPersistence { get; set; }
    public static SqlServerSagaPersistenceConfig? SqlServerSagaPersistence { get; set; }
}