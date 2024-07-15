// namespace Rsb;
//
// internal interface ISagaHolder<TSaga, TSagaData>
//     where TSaga : Saga<TSagaData> 
//     where TSagaData : class, IAmSagaData, new()
// {
//     public IAmSaga Saga { get; }
//     // public IAmSagaData SagaData { get; }
// }
//
// internal class SagaHolder<TSaga, TSagaData> : ISagaHolder<TSaga, TSagaData> 
//     where TSaga : Saga<TSagaData> 
//     where TSagaData : class, IAmSagaData, new()
// {
//     public IAmSaga Saga { get; }
//     // public IAmSagaData SagaData { get; }
//
//     public SagaHolder(IAmSaga saga
//         // , IAmSagaData sagaData
//         )
//     {
//         Saga = saga;
//         // SagaData = sagaData;
//     }
// }
//
// internal interface IAmSaga
// {
//     public IAmSagaData Data { get; }
// }
//
// public class Saga : IAmSaga 
// {
//     public IAmSagaData Data { get; init; }
// }
//
// public abstract class Saga<TSagaData> : Saga 
//     where TSagaData : class, IAmSagaData, new()
// {
//     public TSagaData Data { get; set; }
// }
//
// public interface IAmSagaData
// {
// }
//
// public interface IAmStartedBy<in TMessage> : IHandleMessage<TMessage>
// {
// }
//
// public interface IReplyTo<in TMessage> : IHandleMessage<TMessage>
// {
// }