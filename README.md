# ASB

This is a lightweight messaging framework build on top of Azure Service Bus services

[![Codacy Badge](https://app.codacy.com/project/badge/Grade/e75f90253491454cbf0dfb25c9c7085b)](https://app.codacy.com/gh/ggcol/rsb/dashboard?utm_source=gh&utm_medium=referral&utm_content=&utm_campaign=Badge_grade)
[![Codacy Badge](https://app.codacy.com/project/badge/Coverage/e75f90253491454cbf0dfb25c9c7085b)](https://app.codacy.com/gh/ggcol/ASureBus/dashboard?utm_source=gh&utm_medium=referral&utm_content=&utm_campaign=Badge_coverage)

ASureBus:

[![NuGet version (ASureBus)](https://img.shields.io/nuget/v/ASureBus.svg?style=flat-square)](https://www.nuget.org/packages/ASureBus/)

ASureBus.Abstractions:

[![NuGet version (ASureBus.Abstractions)](https://img.shields.io/nuget/v/ASureBus.Abstractions.svg?style=flat-square)](https://www.nuget.org/packages/ASureBus.Abstractions/)


## Actual dependencies

- ASureBus:                     

    - Azure.Messaging.Servicebus    
    - Microsoft.Extensions.Hosting  
    - Azure.Storage.Blobs           
    - Microsoft.Data.SqlClient      

- ASureBus.Abstractions:

    - :heavy_check_mark: free from dependencies!

## Setup

### Minimal setup

```csharp
await Host
    .CreateDefaultBuilder()
    .UseAsb<ServiceBusSettings>()
    .RunConsoleAsync();
```

A setting class that implements [IConfigureAzureServiceBus](#basic) must be provided.

or:

```csharp
await Host
    .CreateDefaultBuilder()
    .UseAsb(new ServiceBusConfig
    {
        ConnectionString = "connection-string",
        // All the following are optional, they are initialized as default if not mentioned
        TransportType = "", // Default is "AmqpWebSocket"
        MaxRetries = 0, // Default is 3
        DelayInSeconds = 0, // Default is 0.8
        MaxDelayInSeconds = 0, // Default is 60
        TryTimeoutInSeconds = 0, // Default is 300
        ServiceBusRetryMode = "" // Default is "Fixed"
    })
    .RunConsoleAsync();
```

This overload of UseAsb allows you to manually provide a configuration object for the Service Bus:

### More configurations

Few other configurations are available.

#### AsbCache

Asb uses a custom cache to store service bus senders and topic configurations.

Cache may be configured using a setting class that implements [IConfigureAsbCache](#asbcache-1):

```csharp
await Host
    .CreateDefaultBuilder()
    .UseAsb<ServiceBusSettings>()
    .ConfigureAsbCache<CacheSettings>()
    .RunConsoleAsync();
```

or:

```csharp
await Host
    .CreateDefaultBuilder()
    .UseAsb<ServiceBusSettings>()
    .ConfigureAsbCache(new AsbCacheConfig()
    {
        // All these 3 are optional, they are initialized as default if not mentioned
        Expiration = TimeSpan.FromHours(2), // default is 5 minutes
        TopicConfigPrefix = "", // default is "topicConfig"
        ServiceBusSenderCachePrefix = "" // default is "sender"
    })
    .RunConsoleAsync();
```

This overload of ConfigureAsbCache allows you to manually provide a configuration object for AsbCache; you can specify options like cache expiration, topic configuration prefix, and sender cache prefix. All these options are optional and default to predefined values if not provided.

#### Heavy

Heavies are a way to off-load C# properties that may result in a too heavy payload for Azure Service Bus (limits depends on tier).
An Azure Storage Account must be provided for this purpose.

Heavies may be configured used a setting class that implements [IConfigureHeavyProperties](#heavy-properties)

```csharp
await Host
    .CreateDefaultBuilder()
    .UseAsb<ServiceBusSettings>()
    .UseHeavyProps<HeavySettings>()
    .RunConsoleAsync();
```

or:

```csharp
await Host
    .CreateDefaultBuilder()
    .UseAsb<ServiceBusSettings>()
    .UseHeavyProps(new HeavyPropertiesConfig()
    {
        ConnectionString = "",
        Container = ""
    })
    .RunConsoleAsync();
```

This overload of UseHeavyProps allows you to manually provide a configuration object for heavy properties, specifying things like the data storage connection string and container.

#### Saga Persistence

Sagas are persisted, without additional configuration, in a memory cache. **This is NOT a good practice for production scenario**, it is only intended for testing and debug purposes. 

Both SQLServer or an Azure Storage Account can be used to persist sagas (still memory cache is used for quicker load times but the persistence is hereby granted on the chosen storage provider).

##### SQL Server saga persistence

Use a settings class that implements [IConfigureSqlServerSagaPersistence](#sql-server-saga-persistence-1)

```csharp
await Host
    .CreateDefaultBuilder()
    .UseAsb<ServiceBusSettings>()
    .UseSqlServerSagaPersistence<SqlServerSagaPersistenceSettings>()
    .RunConsoleAsync();
```

or:

```csharp
await Host
    .CreateDefaultBuilder()
    .UseAsb<ServiceBusSettings>()
    .UseSqlServerSagaPersistence(new SqlServerSagaPersistenceConfig()
    {
        ConnectionString = ""
    })
    .RunConsoleAsync();
```

This overload of UseSqlServerSagaPersistence allows you to manually provide a configuration object for sql server saga persistence, the connection string is required.

##### Azure Storage Account saga persistence

Use a settings class that implements [IConfigureDataStorageSagaPersistence](#azure-storage-account-saga-persistence-1)

```csharp
await Host
    .CreateDefaultBuilder()
    .UseAsb<ServiceBusSettings>()
    .UseDataStorageSagaPersistence<DataStorageSagaPersistenceSettings>()
    .RunConsoleAsync();
```

or

```csharp
await Host
    .CreateDefaultBuilder()
    .UseAsb<ServiceBusSettings>()
    .UseDataStorageSagaPersistence(new DataStorageSagaPersistenceConfig()
    {
        ConnectionString = "",
        Container = ""
    })
    .RunConsoleAsync();
```

This overload of UseDataStorageSagaPersistence allows you to manually provide a configuration object for azure storage account saga persistence, both configurations are required.

## Apis

### Configurations

#### Basic

create a settings class that implements:
```csharp
public interface IConfigureAzureServiceBus
{
    public string ConnectionString { get; set; }
    /// <summary>
    /// May be "AmqpTcp" or "AmqpWebSockets", default is "AmqpWebSocket".
    /// Maps to Azure.Messaging.ServiceBus.ServiceBusTransportType.
    /// </summary>
    public string? TransportType { get; set; }
    public int? MaxRetries { get; set; }
    public int? DelayInSeconds { get; set; }
    public int? MaxDelayInSeconds { get; set; }
    public int? TryTimeoutInSeconds { get; set; }
    /// <summary>
    /// May be "fixed" or "exponential", default is "fixed".
    /// Maps to Azure.Messaging.ServiceBus.ServiceBusRetryMode.
    /// </summary>
    public string? ServiceBusRetryMode { get; set; }
    public int? MaxConcurrentCalls { get; set; } 
}
```

and bind your configs from appsettings/azure app configuration/other configurations provider.

or use a configuration object:

```csharp
public sealed class ServiceBusConfig : IConfigureAzureServiceBus
{
    public required string ConnectionString { get; set; }
    public string? TransportType { get; set; }
    public int? MaxRetries { get; set; }
    public int? DelayInSeconds { get; set; }
    public int? MaxDelayInSeconds { get; set; }
    public int? TryTimeoutInSeconds { get; set; }
    public string? ServiceBusRetryMode { get; set; }
    public int? MaxConcurrentCalls { get; set; }
}
```

both ways the only required settings is the Service bus Conncetion String, all other settings have a default fallback.

#### AsbCache

create a settings class that implements:

```csharp
public interface IConfigureAsbCache
{
    public TimeSpan? Expiration { get; set; }
    public string? TopicConfigPrefix { get; set; }
    public string? ServiceBusSenderCachePrefix { get; set; }
}
```

and bind your configs from appsettings/azure app configuration/other configurations provider.

or use a configuration object:

```csharp
public sealed class AsbCacheConfig : IConfigureAsbCache
{
    public TimeSpan? Expiration { get; set; }
    public string? TopicConfigPrefix { get; set; }
    public string? ServiceBusSenderCachePrefix { get; set; }
}
```

none of this settings are required, they all have default fallbacks if not provided.

#### Heavy Properties

create a settings class that implements:

```csharp
public interface IConfigureHeavyProperties
{
    public string ConnectionString { get; set; }
    public string Container { get; set; }
}
```

and bind your configs from appsettings/azure app configuration/other configurations provider.

or use a configuration object:

```csharp
public sealed class HeavyPropertiesConfig : IConfigureHeavyProperties
{
    public required string ConnectionString { get; set; }
    public required string Container { get; set; }
}
```

both settings are required for heavies to works.

#### Saga Persistence

##### SQL Server saga persistence

create a settings class that implements:

```csharp
public interface IConfigureSqlServerSagaPersistence
{
    public string? ConnectionString { get; set; }
}
```

and bind your configs from appsettings/azure app configuration/other configurations provider.

or use a configuration object:

```csharp
public class SqlServerSagaPersistenceConfig : IConfigureSqlServerSagaPersistence
{
    public required string ConnectionString { get; set; }
}
```

A SQL Server connection string must be provided.

##### Azure Storage Account saga persistence

create a settings class that implements:

```csharp
public interface IConfigureDataStorageSagaPersistence : IConfigureDataStorage
{
    public string ConnectionString { get; set; }
    public string Container { get; set; }
}
```

and bind your configs from appsettings/azure app configuration/other configurations provider.

or use a configuration object:

```csharp
public sealed class DataStorageSagaPersistenceConfig : ConfigureDataStorageSagaPersistence
{
    public required string ConnectionString { get; set; }
    public required string Container { get; set; }
}
```

both settings are required.

### Messages

#### Commands

```csharp
public interface IAmACommand { }
```
usage example:
```csharp
public class ACommand : IAmACommand
{
    public string? Something { get; init; }
}
```

#### Events

```csharp
public interface IAmAnEvent { }
```
usage example:
```csharp
public class AnEvent : IAmAnEvent
{
    public string? Something { get; set; }
}
```

### Message handling

#### Handlers

```csharp
public interface IHandleMessage<in TMessage>
    where TMessage : IAmAMessage
{
    public Task Handle(TMessage message, IMessagingContext context,
        CancellationToken cancellationToken = default);

    public Task HandleError(Exception ex, IMessagingContext context,
        CancellationToken cancellationToken = default)
    {
        throw ex;
    }
}
```
usage example:
```csharp
public class ACommandHandler : IHandleMessage<ACommand>
{
    public async Task Handle(ACommand message, IMessagingContext context,
        CancellationToken cancellationToken = default)
    {
        //do sth
    }
}
```

#### Sagas

```csharp
public abstract class Saga<T>
    where T : SagaData, new()
{
    public T SagaData { get; } = new();
}
```
```csharp
public abstract class SagaData
{
}
```
usage example:
```csharp
public class ASaga : Saga<ASagaData>,
        IAmStartedBy<ASagaInitCommand>,
        IHandleMessage<AReply>
{
    public async Task Handle(ASagaInitCommand message,
        IMessagingContext context,
        CancellationToken cancellationToken = default)
    {
        //do sth
    }

    public async Task Handle(AReply message, IMessagingContext context,
        CancellationToken cancellationToken = default)
    {
        //do sth

        IAmComplete();
    }
}
```

### Send Messages

```csharp
public interface IMessagingContext
{
    public Guid CorrelationId { get; }
    
    public Task Send<TCommand>(TCommand message,
        CancellationToken cancellationToken = default)
        where TCommand : IAmACommand;

    public Task Publish<TEvent>(TEvent message,
        CancellationToken cancellationToken = default)
        where TEvent : IAmAnEvent;
}
```

#### Send messages from any class

IMessagingContext is provided by default DI container, simply inject it in your class and use it:

```csharp
internal class OneCommandInitJob : IHostedService
{
    private readonly IMessagingContext _context;
    private readonly IHostApplicationLifetime _hostApplicationLifetime;

    public OneCommandInitJob(IMessagingContext context,
        IHostApplicationLifetime hostApplicationLifetime)
    {
        _context = context;
        _hostApplicationLifetime = hostApplicationLifetime;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var max = new Random().Next(5);

        for (var i = 0; i <= max; i++)
        {
            await _context.Send(new ACommand
                {
                    Something = $"{i} - Hello world!"
                }, cancellationToken)
                .ConfigureAwait(false);
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _hostApplicationLifetime.StopApplication();
    }
}
```

#### Send messages within an handler

Simply use the already provided messaging context:

```csharp
public class ACommandHandler : IHandleMessage<ACommand>
{
    public async Task Handle(ACommand message, IMessagingContext context,
        CancellationToken cancellationToken = default)
    {
        await context.Send(new ACommand(), cancellationToken)
            .ConfigureAwait(false);

        await context.Publish(new AnEvent(), cancellationToken)
            .ConfigureAwait(false);
    }
}
```

#### Send options

A part from simple send/publish methods a few overload and options can be used:
_Send method is used for the sake of examples but the same applies to Publish() method(s)_

##### Delayed messages

```csharp
var delay = TimeSpan.FromSeconds(20);

var message = new DelayedMessage();

await context.SendAfter(message, delay, cancellationToken)
    .ConfigureAwait(false);
        
// alternative way to send a delayed message
await context.Send(message, new SendOptions
    {
        Delay = delay
    }, cancellationToken)
    .ConfigureAwait(false);
```

##### Scheduled messages

```csharp
var scheduledTime = new DateTimeOffset(2025, 1, 1, 0, 0, 1, TimeSpan.Zero); 

var message = new ScheduledMessage
{
    Message = "Happy new year!"
};

await context.SendScheduled(message, scheduledTime, cancellationToken)
    .ConfigureAwait(false);

// alternative way to send a scheduled message
await context.Send(message, new SendOptions
    {
        ScheduledTime = scheduledTime
    }, cancellationToken)
    .ConfigureAwait(false);
```

### Heavy Properties

```csharp
public class Heavy<T>() : Heavy
{
    public T? Value { get; set; }

    public Heavy(T value) : this()
    {
        Value = value;
    }
}
```
usage example:
```csharp
public class HeavyCommand : IAmACommand
{
    public Heavy<string> AHeavyProp { get; set; }
}
```

## Notes on serialization

So far serialization is handled by System.Text.Json and no option for a different serializer is exposed.