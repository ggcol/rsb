using Rsb.Configurations;

namespace Rsb.Services;

internal interface ICollectMessage
{
    internal Queue<MessageHolder> Messages { get; }
    internal class MessageHolder
    {
        public string MessageName { get; init; }
        public IAmAMessage Message { get; init; }
    }
}