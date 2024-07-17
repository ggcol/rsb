using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Playground;
using Rsb;
using Rsb.Configurations;
using Rsb.Core;
using Rsb.Services;

await Host
    .CreateDefaultBuilder()
    .UseRsb<Settings>()
    .ConfigureServices(
        (_, services) =>
        {
            services.AddHostedService<InitJob>();
            services.AddLogging();
        })
    .RunConsoleAsync();


public class AMessage : IAmACommand
{
    public string? Something { get; init; }
}

public class Message1 : IAmACommand{}
public class Message2 : IAmACommand{}

public class AnHandler : IHandleMessage<AMessage>
{
    public async Task Handle(AMessage message, IMessagingContext context,
        CancellationToken cancellationToken = default)
    {
        Console.WriteLine(message.Something);
    }
}

// public class AnHandlerDouble : IHandleMessage<Message1>,
//     IHandleMessage<Message2>
// {
//     public Task Handle(Message1 message, IMessagingContext context,
//         CancellationToken cancellationToken = default)
//     {
//         throw new NotImplementedException();
//     }
//
//     public Task Handle(Message2 message, IMessagingContext context,
//         CancellationToken cancellationToken = default)
//     {
//         throw new NotImplementedException();
//     }
// }


internal class InitJob : IHostedService
{
    private readonly IMessagingContext _context;
    private readonly IMessageEmitter _emitter;
    private readonly IHostApplicationLifetime _hostApplicationLifetime;

    public InitJob(IMessagingContext context,
        IMessageEmitter emitter,
        IHostApplicationLifetime hostApplicationLifetime)
    {
        _context = context;
        _emitter = emitter;
        _hostApplicationLifetime = hostApplicationLifetime;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await _context.Send(new AMessage()
            {
                Something = "Hello world!"
            }, cancellationToken)
            .ConfigureAwait(false);
        
        // await _context.Send(new Message1(), cancellationToken)
        //     .ConfigureAwait(false);
        //
        // await _context.Send(new Message2(), cancellationToken)
        //     .ConfigureAwait(false);
        
        await _emitter.FlushAll((ICollectMessage)_context, cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _hostApplicationLifetime.StopApplication();
    }
}
// public class Settings : IConfigureAzureServiceBus
// {
//     public string SbConnectionString { get; set; }
// }
//
// public class MySaga : Saga<MySagaData>
//     , IAmStartedBy<InitMessage>
//     , IReplyTo<FirstReplyMessage>
//     , IReplyTo<SecondReplyMessage>
// {
//     public Task Handle(InitMessage? message, IMessagingContext context,
//         CancellationToken cancellationToken = default)
//     {
//         throw new NotImplementedException();
//     }
//
//     public Task Handle(FirstReplyMessage? message, IMessagingContext context,
//         CancellationToken cancellationToken = default)
//     {
//         throw new NotImplementedException();
//     }
//
//     public Task Handle(SecondReplyMessage? message, IMessagingContext context,
//         CancellationToken cancellationToken = default)
//     {
//         throw new NotImplementedException();
//     }
// }
//
// public class MySagaData : IAmSagaData
// {
// }
//
// public class InitMessage : IAmACommand
// {
// }
//
// public class FirstReplyMessage : IAmACommand
// {
// }
//
// public class SecondReplyMessage : IAmACommand
// {
// }