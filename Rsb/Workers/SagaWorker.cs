// using System.Text.Json;
// using Azure.Messaging.ServiceBus;
// using Microsoft.Extensions.Hosting;
// using Rsb.Configurations;
// using Rsb.Services;
//
// namespace Rsb.Workers;
//
// internal sealed class SagaWorker<TSaga, TSagaData>
//     : IHostedService
//     where TSaga : Saga<TSagaData> 
//     where TSagaData : class, IAmSagaData, new()
// {
//     private readonly ISagaHolder<TSaga, TSagaData> _sagaHolder;
//     private readonly IMessagingContext _messagingContext;
//     private readonly IMessageEmitter _messageEmitter;
//
//     private IDictionary<Type, ServiceBusProcessor> _processors { get; set; } =
//         new Dictionary<Type, ServiceBusProcessor>();
//
//     public SagaWorker(
//         ISagaHolder<TSaga, TSagaData> sagaHolder, 
//         IAzureServiceBusService serviceBusService,
//         IMessagingContext messagingContext, IMessageEmitter messageEmitter)
//     {
//         ConfigureWorker(serviceBusService)
//             .ConfigureAwait(false)
//             .GetAwaiter()
//             .GetResult();
//
//         _sagaHolder = sagaHolder;
//         _messagingContext = messagingContext;
//         _messageEmitter = messageEmitter;
//
//         if (_processors.Any())
//         {
//             foreach (var processor in _processors)
//             {
//                 processor.Value.ProcessMessageAsync += args =>
//                     ProcessMessage(args, processor.Key);
//                 //TODO errors
//             }
//         }
//     }
//
//     private async Task ProcessMessage(ProcessMessageEventArgs args,
//         Type messageType)
//     {
//         var body = args.Message.Body.ToString();
//         var message = JsonSerializer.Deserialize(body, messageType);
//
//         var relativeInterface = _sagaHolder.Saga.GetType()
//             .GetInterfaces()
//             .Single(i =>
//                 i.IsGenericType &&
//                 (
//                     // i.GetGenericTypeDefinition() == typeof(IAmStartedBy<>) ||
//                     // i.GetGenericTypeDefinition() == typeof(IReplyTo<>)
//                     i.GetGenericTypeDefinition() == typeof(IHandleMessage<>)
//                 ) &&
//                 i.GetGenericArguments().Single() == messageType);
//
//         //TODO bleurgh
//         var handleMethod = relativeInterface.GetMethod("Handle");
//
//         handleMethod?.Invoke(_sagaHolder.Saga, new[] { message });
//
//         await args.CompleteMessageAsync(args.Message).ConfigureAwait(false);
//
//         await _messageEmitter.FlushAll((ICollectMessage)_messagingContext,
//                 args.CancellationToken)
//             .ConfigureAwait(false);
//     }
//
//     private async Task ConfigureWorker(
//         IAzureServiceBusService serviceBusService)
//     {
//         var sagaImplementedInterfaces = _sagaHolder.Saga.GetType().GetInterfaces();
//
//         var initInterfaces = sagaImplementedInterfaces
//             .Where(i => i.IsGenericType &&
//                         i.GetGenericTypeDefinition() == typeof(IAmStartedBy<>));
//
//         var replyInterfaces = sagaImplementedInterfaces
//             .Where(i => i.IsGenericType &&
//                         i.GetGenericTypeDefinition() == typeof(IReplyTo<>));
//
//         var initMessageTypes =
//             initInterfaces.Select(i => i.GetGenericArguments());
//
//         var replyMessageTypes =
//             replyInterfaces.Select(i => i.GetGenericArguments());
//
//         await ConfigureMessages(initMessageTypes, serviceBusService)
//             .ConfigureAwait(false);
//
//         await ConfigureMessages(replyMessageTypes, serviceBusService)
//             .ConfigureAwait(false);
//     }
//
//     private async Task ConfigureMessages(IEnumerable<Type[]> messageTypes,
//         IAzureServiceBusService serviceBusService)
//     {
//         foreach (var messageType in messageTypes)
//         {
//             var type = messageType.Single();
//             var messageInterfaces = type.GetInterfaces();
//
//             if (messageInterfaces.Any(i => i == typeof(IAmACommand)))
//             {
//                 var queue = await serviceBusService
//                     .ConfigureQueue(type.Name)
//                     .ConfigureAwait(false);
//
//                 _processors.Add(type, serviceBusService.GetProcessor(queue));
//                 continue;
//             }
//
//             // ReSharper disable once InvertIf
//             if (messageInterfaces.Any(i => i == typeof(IAmAnEvent)))
//             {
//                 var topic = await serviceBusService
//                     .ConfigureTopicForReceiver(type)
//                     .ConfigureAwait(false);
//
//                 _processors.Add(type,
//                     serviceBusService.GetProcessor(topic.Name,
//                         topic.SubscriptionName));
//                 continue;
//             }
//
//             //TODO custom ex?
//             //message must be either a Command or an Event
//             throw new InvalidOperationException();
//         }
//     }
//
//     #region IHosteService implemented
//
//     public async Task StartAsync(CancellationToken cancellationToken)
//     {
//         if (_processors.Any())
//         {
//             foreach (var processor in _processors.Values)
//             {
//                 await processor.StartProcessingAsync(cancellationToken)
//                     .ConfigureAwait(false);
//             }
//         }
//     }
//
//     public async Task StopAsync(CancellationToken cancellationToken)
//     {
//         if (_processors.Any())
//         {
//             foreach (var processor in _processors.Values)
//             {
//                 await processor.StopProcessingAsync(cancellationToken)
//                     .ConfigureAwait(false);
//                 await processor.DisposeAsync().ConfigureAwait(false);
//             }
//         }
//     }
//
//     #endregion
// }