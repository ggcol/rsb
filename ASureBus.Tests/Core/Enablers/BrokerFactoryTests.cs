﻿using ASureBus.Abstractions;
using ASureBus.Accessories.Heavy;
using ASureBus.Core.Enablers;
using ASureBus.Core.TypesHandling.Entities;
using Moq;

namespace ASureBus.Tests.Core.Enablers;

[TestFixture]
public class BrokerFactoryTests
{
    private Mock<IServiceProvider> _serviceProviderMock;
    private Mock<IHeavyIO> _heavyIoMock;

    [SetUp]
    public void SetUp()
    {
        _serviceProviderMock = new Mock<IServiceProvider>();
        _heavyIoMock = new Mock<IHeavyIO>();
    }

    [Test]
    public void Get_HandlerBroker_ShouldReturnHandlerBrokerInstance()
    {
        // Arrange
        var handlerType = new HandlerType
        {
            MessageType = new MessageType
            {
                Type = typeof(FakeMessage),
                IsCommand = true
            },
            Type = typeof(FakeHandler)
        };
        var correlationId = Guid.NewGuid();
        _serviceProviderMock.Setup(sp => sp.GetService(typeof(FakeHandler)))
            .Returns(new FakeHandler());

        // Act
        var broker = BrokerFactory.Get(_serviceProviderMock.Object, handlerType,
            _heavyIoMock.Object, correlationId);

        // Assert
        Assert.That(broker, Is.Not.Null);
        Assert.That(broker, Is.InstanceOf<HandlerBroker<FakeMessage>>());
    }

    [Test]
    public void Get_SagaBroker_ShouldReturnSagaBrokerInstance()
    {
        // Arrange
        var listenerType = new SagaHandlerType
        {
            MessageType = new MessageType
            {
                Type = typeof(FakeMessage),
                IsCommand = true
            },
            IsInitMessage = true
        };

        var sagaType = new SagaType()
        {
            Type = typeof(FakeSaga),
            SagaDataType = typeof(FakeSagaData),
            Listeners = [listenerType]
        };

        var correlationId = Guid.NewGuid();
        var implSaga = new FakeSaga();

        _serviceProviderMock.Setup(sp => sp.GetService(typeof(FakeSaga)))
            .Returns(implSaga);

        // Act
        var broker = BrokerFactory.Get(_serviceProviderMock.Object, sagaType, implSaga,
            listenerType, _heavyIoMock.Object, correlationId);

        // Assert
        Assert.That(broker, Is.Not.Null);
        Assert.That(broker, Is.InstanceOf<SagaBroker<FakeSagaData, FakeMessage>>());
    }

    private class FakeHandler : IHandleMessage<FakeMessage>
    {
        public Task Handle(FakeMessage message, IMessagingContext context,
            CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }

    private class FakeMessage : IAmACommand
    {
    }

    private class FakeSaga : Saga<FakeSagaData>, IAmStartedBy<FakeMessage>
    {
        public Task Handle(FakeMessage message, IMessagingContext context,
            CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }

    private class FakeSagaData : SagaData
    {
    }
}