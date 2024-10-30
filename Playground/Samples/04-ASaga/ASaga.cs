using ASureBus.Abstractions;

namespace Playground.Samples._04_ASaga;

public class ASaga : Saga<ASagaData>,
        IAmStartedBy<ASagaInitCommand>,
        IHandleMessage<AReply>
{
    public async Task Handle(ASagaInitCommand message,
        IMessagingContext context,
        CancellationToken cancellationToken = default)
    {
        var comunication = "Hello world!";
        
        Console.WriteLine($"========== {nameof(ASagaInitCommand)} ==========");
        Console.WriteLine($"{nameof(ASagaInitCommand)} correlationId: {context.CorrelationId}");
        Console.WriteLine($"{nameof(ASagaInitCommand)} message: {comunication}");
        
        await context.Send(new AnotherCommand
            {
                Something = comunication
            }, cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task Handle(AReply message, IMessagingContext context,
        CancellationToken cancellationToken = default)
    {
        Console.WriteLine($"========== {nameof(AReply)} ==========");
        Console.WriteLine($"{nameof(AReply)} correlationId: {context.CorrelationId}");
        Console.WriteLine($"{nameof(AReply)} message: {message.Something}");

        IAmComplete();
    }
}