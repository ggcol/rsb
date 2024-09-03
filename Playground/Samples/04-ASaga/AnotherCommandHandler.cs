using Rsb;

namespace Playground.Samples._04_ASaga;

public class AnotherCommandHandler : IHandleMessage<AnotherCommand>
{
    public async Task Handle(AnotherCommand message, IMessagingContext context,
        CancellationToken cancellationToken = default)
    {
        Console.WriteLine($"========== {nameof(AnotherCommand)} ==========");
        Console.WriteLine($"{nameof(AnotherCommand)} correlationId: {context.CorrelationId}");
        Console.WriteLine($"{nameof(AnotherCommand)} message: {message.Something}");
        await context.Send(new AReply()
        {
            Something = message.Something
        }, cancellationToken)
        .ConfigureAwait(false);
    }
}