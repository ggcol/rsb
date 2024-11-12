using ASureBus.Abstractions.Configurations.ConfigObjects;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Playground.Samples._06_SagaPersistence;
using Playground.Settings;
using ASureBus.Core.DI;
using Playground;
using Playground.Samples._01_OneCommand;
using Playground.Samples._02_OneEvent;
using Playground.Samples._03_TwoMessagesSameHandlerClass;
using Playground.Samples._04_ASaga;
using Playground.Samples._05_Heavy;
using Playground.Samples._07_DelayedAndScheduled;
using Playground.Samples._08_ABrokenSaga;

await Host
    .CreateDefaultBuilder()
    
    
    
    /*
     * ========================================
     * MINIMAL SETUP
     * ========================================
     */

    // Configure the application to use Azure Service Bus with the specified settings
    .UseAsb<ServiceBusSettings>()

    // Configure the application to use Azure Service Bus with a custom configuration
    // .UseAsb(new ServiceBusConfig()
    // {
    //     ServiceBusConnectionString = "",
    //     ClientOptions = new()
    //     {
    //         TransportType = ServiceBusTransportType.AmqpWebSockets
    //     }
    // })
    
    
    
    /*
     * ========================================
     * HEAVY PROPS SETUP
     * ========================================
     */

    // Configure the application to use heavy properties with the specified settings
    // .UseHeavyProps<HeavySettings>()

    // Configure the application to use heavy properties with a custom configurationx
    // .UseHeavyProps(new HeavyPropertiesConfig()
    // {
    //     DataStorageConnectionString = "",
    //     DataStorageContainer = ""
    // })
    
    
    
    /*
     * ========================================
     * SAGA PERSISTENCE SETUP
     * ========================================
     */

    // Configure the application to use data storage for saga persistence with the specified settings
    // .UseDataStorageSagaPersistence<DataStorageSagaPersistenceSettings>()

    // Configure the application to use SQL Server for saga persistence with the specified settings
    // .UseSqlServerSagaPersistence<SqlServerSagaPersistenceSettings>()
    
    
    
    /*
     * ========================================
     * CONFIGURE APPLICATION SERVICES
     * ========================================
     */

    // Configure the application's services
    .ConfigureServices(
        (_, services) =>
        {
            services.AddHostedService<OneCommandInitJob>();
            // services.AddHostedService<OneEventInitJob>();
            // services.AddHostedService<TwoMessagesSameHandlerClassInitJob>();
            // services.AddHostedService<ASagaInitJob>();
            // services.AddHostedService<AGenericJob>();
            // services.AddHostedService<HeavyInitJob>();
            // services.AddHostedService<APersistedSagaInitJob>();
            // services.AddHostedService<DelayedAndScheduledInitJob>();
            // services.AddHostedService<ABrokenSagaInitJob>();
            services.AddLogging();
        })
    
    
    
    /*
     * ========================================
     * CACHE SETUP
     * ========================================
     */

    // Configure the application to use Azure Service Bus cache with the specified settings
    // .ConfigureAsbCache<WholeCacheSettings>()
    // .ConfigureAsbCache<PartialCacheSettings>()

    // Configure the application to use Azure Service Bus cache with a custom configuration
    // .ConfigureAsbCache(new AsbCacheConfig()
    // {
    //     // All these 3 are optional, they are initialized as default if not mentioned
    //     Expiration = TimeSpan.FromHours(2),
    //     TopicConfigPrefix = "",
    //     ServiceBusSenderCachePrefix = ""
    // })
    
    
    
    /*
     * ========================================
     * RUN THE APPLICATION
     * ========================================
     */
    // Run the application as a console application
    .RunConsoleAsync();