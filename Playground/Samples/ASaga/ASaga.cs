using Rsb;

namespace Playground.Samples.ASaga;

public class ASaga
    : NewSaga<ASagaData>,
        IAmStartedBy<ASagaInitCommand>,
        IHandleMessage<AnotherCommand>
{
    public async Task Handle(ASagaInitCommand message,
        IMessagingContext context,
        CancellationToken cancellationToken = default)
    {
        await context.Send(new AnotherCommand()
            {
                Something = "Hello world!"
            }, cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task Handle(AnotherCommand message, IMessagingContext context,
        CancellationToken cancellationToken = default)
    {
        Console.WriteLine(message.Something);
        IAmComplete();
    }
}