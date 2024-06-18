using rsb.Configurations;

namespace rsb.Services;

internal interface ICollectMessage
{
    internal Queue<MessageHolder> Messages { get; }
    internal class MessageHolder
    {
        public string MessageName { get; init; }
        public IAmAMessage Message { get; init; }
    }
}