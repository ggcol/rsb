using Asb.Core.Entities;

namespace Asb.Core.Messaging;

internal interface ICollectMessage
{
    internal Queue<IRsbMessage> Messages { get; }
}