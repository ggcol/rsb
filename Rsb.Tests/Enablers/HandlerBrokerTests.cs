// using Moq;
// using Rsb.Core.Enablers;
//
// namespace Rsb.Tests.Enablers;
//
// [TestFixture]
// public class HandlerBrokerTests
// {
//     private Mock<IHandleMessage<IAmAMessage>> _handlerMock;
//     private Mock<IMessagingContext> _contextMock;
//     private HandlerBroker<IAmAMessage> _handlerBroker;
//
//     [SetUp]
//     public void SetUp()
//     {
//         _handlerMock = new Mock<IHandleMessage<IAmAMessage>>();
//         _contextMock = new Mock<IMessagingContext>();
//         _handlerBroker = new HandlerBroker<IAmAMessage>(_handlerMock.Object,
//             _contextMock.Object);
//     }
//     
//     [Test]
//     public async Task HandleError_GivenException_HandlesError()
//     {
//         // Arrange
//         var exception = new Exception("Test exception");
//
//         _handlerMock.Setup(h =>
//                 h.HandleError(
//                     It.IsAny<Exception>(),
//                     It.IsAny<IMessagingContext>(),
//                     It.IsAny<CancellationToken>()))
//             .Returns(Task.CompletedTask);
//
//         // Act
//         await _handlerBroker.HandleError(exception);
//
//         // Assert
//         _handlerMock.Verify(
//             h => h.HandleError(exception, _contextMock.Object,
//                 It.IsAny<CancellationToken>()), Times.Once);
//     }
// }