using Rsb.Core.Enablers.Entities;

namespace Rsb.Services;

internal interface ICollectMessage
{
    internal Queue<IRsbMessage> Messages { get; }
}