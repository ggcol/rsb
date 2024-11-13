# ASB

This is a lightweight messaging framework build on top of Azure Service Bus services

[![Codacy Badge](https://app.codacy.com/project/badge/Grade/e75f90253491454cbf0dfb25c9c7085b)](https://app.codacy.com/gh/ggcol/rsb/dashboard?utm_source=gh&utm_medium=referral&utm_content=&utm_campaign=Badge_grade)
[![Codacy Badge](https://app.codacy.com/project/badge/Coverage/e75f90253491454cbf0dfb25c9c7085b)](https://app.codacy.com/gh/ggcol/ASureBus/dashboard?utm_source=gh&utm_medium=referral&utm_content=&utm_campaign=Badge_coverage)

ASureBus:

[![NuGet version (ASureBus)](https://img.shields.io/nuget/v/ASureBus.svg?style=flat-square)](https://www.nuget.org/packages/ASureBus/)

ASureBus.Abstractions:

[![NuGet version (ASureBus.Abstractions)](https://img.shields.io/nuget/v/ASureBus.Abstractions.svg?style=flat-square)](https://www.nuget.org/packages/ASureBus.Abstractions/)


## Actual dependencies:

ASureBus:                     

- Azure.Messaging.Servicebus    
- Microsoft.Extensions.Hosting  
- Azure.Storage.Blobs           
- Microsoft.Data.SqlClient      

ASureBus.Abstractions:

free from dependencies :heavy_check_mark:

## Minimal setup:
```csharp
await Host
    .CreateDefaultBuilder()
    .UseAsb<ServiceBusSettings>()
    .RunConsoleAsync();
```

A setting class that implements IConfigureAzureServiceBus must be provided.

This overload of UseAsb allows you to manually provide a configuration object for the Service Bus:
```csharp
await Host
    .CreateDefaultBuilder()
    .UseAsb(new ServiceBusConfig()
    {
        ServiceBusConnectionString = "",
        //this is optional, default will be used otherwise
        ClientOptions = new ServiceBusClientOptions()
    })
    .RunConsoleAsync();
```

### More configurations:
Few other configurations are available.

#### AsbCache:
Asb uses a custom cache to store service bus senders and topic configurations.
These are the default configurations:

| Config                      | Default     |
|-----------------------------|-------------|
| Expiration                  | 5 minutes   |
| Topic Config Prefixes       | topicConfig |
| Service Bus Sender Prefixes | sender      |

Cache is fully configurable:
```csharp
await Host
    .CreateDefaultBuilder()
    .UseAsb<ServiceBusSettings>()
    .ConfigureAsbCache<CacheSettings>()
    .RunConsoleAsync();
```
using a setting class that implements IConfigureAsbCache, or:
```csharp
await Host
    .CreateDefaultBuilder()
    .UseAsb<ServiceBusSettings>()
    .ConfigureAsbCache(new AsbCacheConfig()
    {
        //all these 3 are optional, they are init as default if not mentioned
        Expiration = TimeSpan.FromHours(2),
        TopicConfigPrefix = "",
        ServiceBusSenderCachePrefix = ""
    })
    .RunConsoleAsync();
```
you can specify options like cache expiration, topic configuration prefix, and sender cache prefix. All these options are optional and default to predefined values if not provided.

#### Heavy:
Heavy are a way to off-load C# properties that result in a too heavy payload for Azure Service Bus (limits depends on tier).
An Azure Storage Account must be provided for this purpose.
```csharp
await Host
    .CreateDefaultBuilder()
    .UseAsb<ServiceBusSettings>()
    .UseHeavyProps<HeavySettings>()
    .RunConsoleAsync();
```
using a setting class that implements IConfigureHeavyProperties, or:
```csharp
await Host
    .CreateDefaultBuilder()
    .UseAsb<ServiceBusSettings>()
    .UseHeavyProps(new HeavyPropertiesConfig()
    {
        DataStorageConnectionString = "",
        DataStorageContainer = ""
    })
    .RunConsoleAsync();
```
 manually provide a configuration object for heavy properties, specifying things like the data storage connection string and container.

## Apis:

### Configurations:
#### Basic:

```csharp
public interface IConfigureAzureServiceBus
{
    public string? ServiceBusConnectionString { get; set; }
}
```

```csharp
public sealed class ServiceBusConfig : IConfigureAzureServiceBus
{
    public string? ServiceBusConnectionString { get; set; }
    public ServiceBusClientOptions ClientOptions { get; set; } = DEFAULT_CLIENT_OPTIONS;
}
```

#### AsbCache:
```csharp
public interface IConfigureAsbCache
{
    public TimeSpan? Expiration { get; set; }
    public string? TopicConfigPrefix { get; set; }
    public string? ServiceBusSenderCachePrefix { get; set; }
}
```
```csharp
public sealed class AsbCacheConfig : IConfigureAsbCache
{
    public TimeSpan? Expiration { get; set; }
    public string? TopicConfigPrefix { get; set; }
    public string? ServiceBusSenderCachePrefix { get; set; }
}
```

#### Heavy Properties:
```csharp
public interface IConfigureHeavyProperties
{
    public string? DataStorageConnectionString { get; set; }
    public string? DataStorageContainer { get; set; }
}
```
```csharp
public sealed class HeavyPropertiesConfig : IConfigureHeavyProperties
{
    public string? DataStorageConnectionString { get; set; }
    public string? DataStorageContainer { get; set; }
}
```
### Messages:
#### Commands:
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
#### Events:
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
### Message handling:
#### Handlers:
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
#### Sagas:
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

### Send Messages:
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
usage example:
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

#### within an handler:
simply use the already provided messaging context:
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

### Heavy Properties:
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
