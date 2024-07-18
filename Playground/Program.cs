using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Playground;
using Playground.Samples.OneCommand;
using Playground.Samples.OneEvent;
using Rsb.Core;

await Host
    .CreateDefaultBuilder()
    .UseRsb<Settings>()
    .ConfigureServices(
        (_, services) =>
        {
            // services.AddHostedService<OneCommandInitJob>();
            services.AddHostedService<OneEventInitJob>();
            // services.AddHostedService<TwoMessagesSameHandlerClassInitJob>();
            // services.AddHostedService<AGenericJob>();
            services.AddLogging();
        })
    .RunConsoleAsync();

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