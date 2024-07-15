using System.Text.Json;
using Azure.Messaging.ServiceBus;
using Rsb.Configurations;

namespace Rsb.Services;

internal sealed class MessageEmitter : IMessageEmitter
{
    private readonly IAzureServiceBusService _serviceBusService;

    public MessageEmitter(IAzureServiceBusService serviceBusService)
    {
        _serviceBusService = serviceBusService;
    }
    
    public async Task FlushAll(ICollectMessage collector,
        CancellationToken cancellationToken = default)
    {
        while (collector.Messages.Any())
        {
            try
            {
                var holder = collector.Messages.FirstOrDefault();

                var destination = holder.Message is IAmACommand
                    ? await _serviceBusService
                        .ConfigureQueue(holder.MessageName)
                        .ConfigureAwait(false)
                    : await _serviceBusService
                        .ConfigureTopicForSender(holder.MessageName)
                        .ConfigureAwait(false);

                await Emit(holder.Message, destination, cancellationToken)
                    .ConfigureAwait(false);
                
                collector.Messages.Dequeue();
            }
            catch (Exception ex)
                /*
                 * May be:
                 * - ServiceBusException (message exceed maximum size)
                 * - SerializationException (non-parsable token)
                 * - ArgumentException (error in sb connection string)
                 */
            {
                //TODO notify AMS - save message for future reference
            }
        }
    }
    
    private async Task Emit(IAmAMessage message, string destination,
        CancellationToken cancellationToken = default)
    {
        var sender = _serviceBusService.GetSender(destination);

        var sbm = CreateMessage(message);

        await sender.SendMessageAsync(sbm, cancellationToken)
            .ConfigureAwait(false);
    }

    //TODO rename
    private static ServiceBusMessage CreateMessage(object message)
    {
        var serialized = JsonSerializer.Serialize(message);
        return new ServiceBusMessage(serialized);
    }
}