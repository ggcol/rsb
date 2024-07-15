using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Hosting;
using Rsb.Services;
using Rsb.Utils;
using Rsb.Workers;

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

        var messagesTypes = new List<Type>();

        var listeners = AssemblySearcher.GetListeners();

        foreach (var listener in listeners)
        {
            var implInterfaces = listener.GetInterfaces();

            foreach (var @interface in implInterfaces)
            {
                if (@interface.GetGenericTypeDefinition() ==
                    typeof(IHandleMessage<>))
                {
                    messagesTypes.Add(@interface.GetGenericArguments().First());
                }
            }
        }

        foreach (var messageType in messagesTypes)
        {
            //TODO subscriptions?
            var processor = azureServiceBusService.GetProcessor(messageType.Name);

            var brokerType = typeof(IListenerBroker<>).MakeGenericType(messageType);

            var broker = _serviceProvider.GetService(brokerType) as IListenerBroker;
            
            //TODO extract to method
            processor.ProcessMessageAsync += async (args) =>
            {
                await broker
                    .Handle(args.Message.Body, args.CancellationToken)
                    .ConfigureAwait(false);

                await args
                    .CompleteMessageAsync(args.Message, args.CancellationToken)
                    .ConfigureAwait(false);

                await _messageEmitter.FlushAll(broker.Collector,
                    args.CancellationToken).ConfigureAwait(false);
            };

            //TODO extract to method
            processor.ProcessErrorAsync += async (args) =>
            {
                await broker.HandleError(args.Exception, args.CancellationToken)
                    .ConfigureAwait(false);

                //TODO complete message? check ms-docs
            };
            
            _processors.Add(messageType, processor);
        }
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