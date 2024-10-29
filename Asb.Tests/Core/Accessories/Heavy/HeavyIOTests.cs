using Asb.Abstractions;
using Moq;
using Asb.Accessories.Heavy;
using Asb.Core.Messaging;
using Asb.Services.StorageAccount;

namespace Asb.Tests.Core.Accessories.Heavy;

// ReSharper disable once InconsistentNaming
[TestFixture]
public class HeavyIOTests
{
    [Test]
    public async Task Unload_ReturnsEmptyListWhenNoHeavyProperties()
    {
        // Arrange
        var storageMock = new Mock<IAzureDataStorageService>();
        var message = new Mock<IAmAMessage>().Object;
        var heavyIo = new HeavyIO(storageMock.Object);

        // Act
        var result = await heavyIo.Unload(message, Guid.NewGuid());

        // Assert
        Assert.That(result, Is.Empty);
    }

    private class AFakeMessage : IAmAMessage
    {
        public Heavy<string> AStringProp { get; set; }
    }

    [Test]
    public async Task Unload_SavesHeavyPropertiesAndReturnsReferences()
    {
        // Arrange
        var messageId = Guid.NewGuid();

        var storageMock = new Mock<IAzureDataStorageService>();
        var message = new AFakeMessage
        {
            AStringProp = new Heavy<string>("AString")
        };

        var heavyIo = new HeavyIO(storageMock.Object);

        var expectedHeavyRef = new HeavyRef
        {
            PropName = nameof(AFakeMessage.AStringProp),
            Ref = message.AStringProp.Ref
        };

        storageMock
            .Setup(s =>
                s.Save(
                    It.IsAny<object>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    false,
                    It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await heavyIo.Unload(message, messageId);

        // Assert
        Assert.That(result.Count, Is.EqualTo(1));
        Assert.Multiple(() =>
        {
            Assert.That(result[0].PropName, Is.EqualTo(expectedHeavyRef.PropName));
            Assert.That(result[0].Ref, Is.EqualTo(expectedHeavyRef.Ref));
        });
    }

    [Test]
    public async Task Unload_SetsHeavyPropertyToNullAfterSaving()
    {
        // Arrange
        var messageId = Guid.NewGuid();

        var storageMock = new Mock<IAzureDataStorageService>();
        var message = new AFakeMessage
        {
            AStringProp = new Heavy<string>("AString")
        };

        var heavyIo = new HeavyIO(storageMock.Object);

        storageMock
            .Setup(s =>
                s.Save(
                    It.IsAny<object>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    false,
                    It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        storageMock
            .Setup(s =>
                s.Save(
                    It.IsAny<object>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    false,
                    It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await heavyIo.Unload(message, messageId);

        // Assert
        var heavyProp = message.GetType().GetProperty(nameof(AFakeMessage.AStringProp));
        var value = heavyProp.GetValue(message);

        Assert.That(value, Is.Null);
    }

    [Test]
    public async Task Load_SetsHeavyPropertiesFromStorage()
    {
        // Arrange
        var messageId = Guid.NewGuid();
        var heavyRef = new HeavyRef
            { PropName = nameof(AFakeMessage.AStringProp), Ref = Guid.NewGuid() };
        var heavies = new List<HeavyRef> { heavyRef };

        var storageMock = new Mock<IAzureDataStorageService>();
        var message = new AFakeMessage();

        var heavyIo = new HeavyIO(storageMock.Object);

        var expectedHeavy = new Heavy<string>("AString");

        storageMock
            .Setup(s => s.Get(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<Type>(),
                    null,
                    CancellationToken.None
                )
            )
            .ReturnsAsync(expectedHeavy);

        storageMock
            .Setup(s => s.Delete(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await heavyIo.Load(message, heavies, messageId);

        // Assert
        var heavyProp = message.GetType().GetProperty(nameof(AFakeMessage.AStringProp));
        var value = heavyProp.GetValue(message) as Heavy<string>;

        Assert.Multiple(() =>
        {
            Assert.That(value, Is.Not.Null);
            Assert.That(value.Value, Is.EqualTo(expectedHeavy.Value));
        });
    }

    [Test]
    public async Task Load_DeletesHeavyPropertiesFromStorageAfterSetting()
    {
        // Arrange
        var messageId = Guid.NewGuid();
        var heavyRef = new HeavyRef
            { PropName = nameof(AFakeMessage.AStringProp), Ref = Guid.NewGuid() };
        var heavies = new List<HeavyRef> { heavyRef };

        var storageMock = new Mock<IAzureDataStorageService>();
        var message = new AFakeMessage();

        var heavyIo = new HeavyIO(storageMock.Object);

        var expectedHeavy = new Heavy<string>("AString");

        storageMock
            .Setup(s => s.Get(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<Type>(),
                    null,
                    CancellationToken.None
                )
            )
            .ReturnsAsync(expectedHeavy);

        storageMock
            .Setup(s => s.Delete(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await heavyIo.Load(message, heavies, messageId);

        // Assert
        storageMock.Verify(s => s.Delete(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}