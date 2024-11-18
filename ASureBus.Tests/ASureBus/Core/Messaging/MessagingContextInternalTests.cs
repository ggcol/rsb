using ASureBus.Abstractions;
using ASureBus.Abstractions.Options.Messaging;
using ASureBus.Core.Messaging;
using Moq;

namespace ASureBus.Tests.ASureBus.Core.Messaging;

public class MessagingContextInternalTests
{
    [Test]
    public async Task Send_ShouldEnqueueMessage()
    {
        // Arrange
        var message = new Mock<IAmACommand>();
        var messagingContext = new MessagingContextInternal();

        // Act
        await messagingContext.Send(message.Object);

        // Assert
        Assert.That(messagingContext.Messages, Has.Count.EqualTo(1));
    }

    [Test]
    public async Task Send_WithOptions_ShouldEnqueueMessage()
    {
        // Arrange
        var message = new Mock<IAmACommand>();
        var options = new SendOptions();
        var messagingContext = new MessagingContextInternal();

        // Act
        await messagingContext.Send(message.Object, options);

        // Assert
        Assert.That(messagingContext.Messages, Has.Count.EqualTo(1));
    }

    [Test]
    public async Task Send_WithOptions_ShouldEnqueueScheduledMessage()
    {
        // Arrange
        var message = new Mock<IAmACommand>();
        var options = new SendOptions
        {
            ScheduledTime = DateTimeOffset.UtcNow.AddSeconds(10)
        };
        var messagingContext = new MessagingContextInternal();

        // Act
        await messagingContext.Send(message.Object, options);

        // Assert
        Assert.That(messagingContext.Messages, Has.Count.EqualTo(1));
    }

    [Test]
    public async Task SendAfter_ShouldEnqueueScheduledMessage()
    {
        // Arrange
        var message = new Mock<IAmACommand>();
        var delay = TimeSpan.FromMinutes(10);
        var messagingContext = new MessagingContextInternal();

        // Act
        await messagingContext.SendAfter(message.Object, delay);

        // Assert
        Assert.That(messagingContext.Messages, Has.Count.EqualTo(1));
    }

    [Test]
    public async Task Publish_ShouldEnqueueMessage()
    {
        // Arrange
        var message = new Mock<IAmAnEvent>();
        var messagingContext = new MessagingContextInternal();

        // Act
        await messagingContext.Publish(message.Object);

        // Assert
        Assert.That(messagingContext.Messages, Has.Count.EqualTo(1));
    }

    [Test]
    public async Task Publish_WithOptions_ShouldEnqueueMessage()
    {
        // Arrange
        var message = new Mock<IAmAnEvent>();
        var options = new PublishOptions();
        var messagingContext = new MessagingContextInternal();

        // Act
        await messagingContext.Publish(message.Object, options);

        // Assert
        Assert.That(messagingContext.Messages, Has.Count.EqualTo(1));
    }

    [Test]
    public async Task Publish_WithOptions_ShouldEnqueueScheduledMessage()
    {
        // Arrange
        var message = new Mock<IAmAnEvent>();
        var options = new PublishOptions
        {
            ScheduledTime = DateTimeOffset.UtcNow.AddSeconds(10)
        };
        var messagingContext = new MessagingContextInternal();

        // Act
        await messagingContext.Publish(message.Object, options);

        // Assert
        Assert.That(messagingContext.Messages, Has.Count.EqualTo(1));
    }

    [Test]
    public async Task PublishAfter_ShouldEnqueueScheduledMessage()
    {
        // Arrange
        var message = new Mock<IAmAnEvent>();
        var delay = TimeSpan.FromMinutes(10);
        var messagingContext = new MessagingContextInternal();

        // Act
        await messagingContext.PublishAfter(message.Object, delay);

        // Assert
        Assert.That(messagingContext.Messages, Has.Count.EqualTo(1));
    }
}