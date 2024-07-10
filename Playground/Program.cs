using Microsoft.Extensions.Hosting;
using Rsb;
using Rsb.Configurations;
using Rsb.HostBuilders;

await OrchestratorHost
    .Create<Settings>()
    .ConfigureServices((services) =>
    {
    })
    .RunConsoleAsync();


public class Settings : IConfigureAzureServiceBus
{
    public string SbConnectionString { get; set; }
}

public class MySaga : Saga<MySagaData>
    , IAmStartedBy<InitMessage>
    , IReplyTo<FirstReplyMessage>
    , IReplyTo<SecondReplyMessage>
{
    public Task Handle(InitMessage message, IMessagingContext context)
    {
        throw new NotImplementedException();
    }

    public Task Handle(FirstReplyMessage message, IMessagingContext context)
    {
        throw new NotImplementedException();
    }

    public Task Handle(SecondReplyMessage message, IMessagingContext context)
    {
        throw new NotImplementedException();
    }
}

public class MySagaData : IAmSagaData
{
}

public class InitMessage : IAmACommand
{
}

public class FirstReplyMessage : IAmACommand
{
}

public class SecondReplyMessage : IAmACommand
{
}