using ASureBus.Core.Entities;
using ASureBus.Services.ServiceBus;
using ASureBus.Utils;
using Azure.Messaging.ServiceBus;

namespace ASureBus.Core.Messaging;

internal sealed class MessageEmitter(IAzureServiceBusService serviceBusService)
    : IMessageEmitter
{
    public async Task FlushAll(ICollectMessage collector,
        CancellationToken cancellationToken = default)
    {
        while (collector.Messages.Count != 0)
        {
            try
            {
                var rsbMessage = collector.Messages.First();

                var destination = rsbMessage.IsCommand
                    ? await serviceBusService
                        .ConfigureQueue(rsbMessage.MessageName, cancellationToken)
                        .ConfigureAwait(false)
                    : await serviceBusService
                        .ConfigureTopicForSender(rsbMessage.MessageName, cancellationToken)
                        .ConfigureAwait(false);

                if (rsbMessage.IsScheduled)
                {
                    await EmitScheduled(rsbMessage, destination, rsbMessage.ScheduledTime!.Value, 
                            cancellationToken)
                        .ConfigureAwait(false);
                }
                else
                {
                    await Emit(rsbMessage, destination, cancellationToken)
                        .ConfigureAwait(false);
                }

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
                //TODO notify maintenance - save message for future reference
            }
        }
    }

    private async Task Emit(IRsbMessage message, string destination,
        CancellationToken cancellationToken)
    {
        var sender = serviceBusService.GetSender(destination);
        
        var sbm = ToSdkMessage(message);

        await sender.SendMessageAsync(sbm, cancellationToken)
            .ConfigureAwait(false);
    }
    
    private async Task EmitScheduled(IRsbMessage message, string destination,
        DateTimeOffset delay, CancellationToken cancellationToken)
    {
        var sender = serviceBusService.GetSender(destination);
        
        var sbm = ToSdkMessage(message);

        await sender.ScheduleMessageAsync(sbm, delay, cancellationToken)
            .ConfigureAwait(false);
    }

    private static ServiceBusMessage ToSdkMessage(object message)
    {
        var serialized = Serializer.Serialize(message);
        return new ServiceBusMessage(serialized);
    }
}