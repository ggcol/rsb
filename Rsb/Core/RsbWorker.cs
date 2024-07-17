using System.Reflection;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Rsb.Enablers;
using Rsb.Services;
using Rsb.Utils;

namespace Rsb.Core;

internal class RsbWorker : IHostedService
{
    private readonly IHostApplicationLifetime _hostApplicationLifetime;
    private readonly IServiceProvider _serviceProvider;
    private readonly IMessageEmitter _messageEmitter;

    private readonly IDictionary<Type, ServiceBusProcessor> _processors = new
        Dictionary<Type, ServiceBusProcessor>();

    public RsbWorker(
        IHostApplicationLifetime hostApplicationLifetime,
        IServiceProvider serviceProvider,
        IAzureServiceBusService azureServiceBusService,
        IMessageEmitter messageEmitter)
    {
        _hostApplicationLifetime = hostApplicationLifetime;
        _serviceProvider = serviceProvider;
        _messageEmitter = messageEmitter;


        var listeners =
            AssemblySearcher.GetIHandleMessageImplementersTypes(
                Assembly.GetEntryAssembly());

        var messagesTypes =
            AssemblySearcher.GetIHandleMessageImplementersMessageTypes(
                listeners);

        foreach (var messageType in messagesTypes)
        {
            //TODO subscriptions?
            var processor =
                azureServiceBusService.GetProcessor(messageType.Name);

            processor.ProcessMessageAsync += async (args) 
                => await ProcessMessage(messageType, args);

            processor.ProcessErrorAsync += async (args)
                => await ProcessError(messageType, args);

            _processors.Add(messageType, processor);
        }
    }

    private async Task ProcessError(Type messageType,
        ProcessErrorEventArgs args)
    {
        var broker = GetBroker(_serviceProvider, messageType);
        await broker.HandleError(args.Exception, args.CancellationToken)
            .ConfigureAwait(false);
    }

    private async Task ProcessMessage(Type messageType,
        ProcessMessageEventArgs args)
    {
        var broker = GetBroker(_serviceProvider, messageType);
        await broker
            .Handle(args.Message.Body, args.CancellationToken)
            .ConfigureAwait(false);

        await args
            .CompleteMessageAsync(args.Message, args.CancellationToken)
            .ConfigureAwait(false);

        await _messageEmitter.FlushAll(broker.Collector,
            args.CancellationToken).ConfigureAwait(false);
    }
    
    /*
     * Probably the same can be done for handlers as well but
     * may be a little quirky.
     * Handler shouldn't have dependencies that cannot be resolved from DI
     * and it is possible to search the entry assembly to get the class
     * that implements IHandleMessage<> of a given messagetype.
     * Once gathered a new instance can be created with ActivatorUtilities
     * and then passed as parameter once creating the broker instance.
     */
    private static IListenerBroker GetBroker(IServiceProvider serviceProvider,
        Type messageType)
    {
        var implType = typeof(ListenerBroker<>).MakeGenericType(messageType);
        return (IListenerBroker)ActivatorUtilities.CreateInstance(
            serviceProvider, implType);
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        foreach (var processor in _processors)
        {
            await processor.Value.StartProcessingAsync(cancellationToken)
                .ConfigureAwait(false);
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        foreach (var processorKvp in _processors)
        {
            await processorKvp.Value.StopProcessingAsync(cancellationToken)
                .ConfigureAwait(false);
            await processorKvp.Value.DisposeAsync().ConfigureAwait(false);
        }

        _hostApplicationLifetime.StopApplication();
    }
}