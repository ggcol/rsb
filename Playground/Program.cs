using rsb;
using rsb.Workers;

var sagaWorker = new SagaWorker<MySaga, MySagaData>(new MySaga());


public class MySaga : Saga<MySagaData>, IAmStartedBy<InitMessage>
{
    public Task Handle(InitMessage message, IMessagingContext context)
    {
        throw new NotImplementedException();
    }
}

public class MySagaData
{
    
}

public class InitMessage{}