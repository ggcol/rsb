using System.Text.Json;
using Azure.Messaging.ServiceBus;
using Rsb.Core.Enablers.Entities;
using Rsb.Services;

namespace Rsb.Core.Messaging;

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
                var rsbMessage = collector.Messages.FirstOrDefault();

                var destination = rsbMessage.IsCommand
                    ? await _serviceBusService
                        .ConfigureQueue(rsbMessage.MessageName,
                            cancellationToken)
                        .ConfigureAwait(false)
                    : await _serviceBusService
                        .ConfigureTopicForSender(rsbMessage.MessageName,
                            cancellationToken)
                        .ConfigureAwait(false);

                await Emit(rsbMessage, destination, cancellationToken)
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

    private async Task Emit(IRsbMessage message, string destination,
        CancellationToken cancellationToken)
    {
        var sender = _serviceBusService.GetSender(destination);

        var sbm = ToSdkMessage(message);

        await sender.SendMessageAsync(sbm, cancellationToken)
            .ConfigureAwait(false);
    }

    private static ServiceBusMessage ToSdkMessage(object message)
    {
        var serialized = JsonSerializer.Serialize(message);
        return new ServiceBusMessage(serialized);
    }
}