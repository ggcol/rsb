using ASureBus.Core.TypesHandling.Entities;
using ASureBus.Services.SqlServer;
using Moq;

namespace ASureBus.Tests.ASureBus.Services.SqlServer;

[TestFixture]
public class SagaSqlServerPersistenceServiceTests
{
    private Mock<ISqlServerService> _mockStorage;
    private SagaSqlServerPersistenceService _service;

    [SetUp]
    public void SetUp()
    {
        _mockStorage = new Mock<ISqlServerService>();
        _service = new SagaSqlServerPersistenceService(_mockStorage.Object);
    }

    private SagaType MakeSagaType()
    {
        return new SagaType()
        {
            Type = typeof(object),
            SagaDataType = typeof(object),
            Listeners = new HashSet<SagaHandlerType>()
        };
    }
    
    [Test]
    public async Task Get_ShouldReturnObject_WhenSagaExists()
    {
        // Arrange
        var sagaType = MakeSagaType();
        var correlationId = Guid.NewGuid();
        var expectedObject = new object();

        _mockStorage.Setup(s => s.Get(sagaType, correlationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedObject);

        // Act
        var result = await _service.Get(sagaType, correlationId);

        // Assert
        Assert.That(result, Is.EqualTo(expectedObject));
    }

    [Test]
    public async Task Get_ShouldReturnNull_WhenSagaDoesNotExist()
    {
        // Arrange
        var sagaType = MakeSagaType();
        var correlationId = Guid.NewGuid();

        _mockStorage.Setup(s => s.Get(sagaType, correlationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((object?)null);

        // Act
        var result = await _service.Get(sagaType, correlationId);

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task Save_ShouldCallStorageSave()
    {
        // Arrange
        var sagaType = MakeSagaType();
        var correlationId = Guid.NewGuid();
        var item = new object();

        // Act
        await _service.Save(item, sagaType, correlationId);

        // Assert
        _mockStorage.Verify(
            s => s.Save(item, sagaType, correlationId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Delete_ShouldCallStorageDelete()
    {
        // Arrange
        var sagaType = MakeSagaType();
        var correlationId = Guid.NewGuid();

        // Act
        await _service.Delete(sagaType, correlationId);

        // Assert
        _mockStorage.Verify(s => s.Delete(sagaType, correlationId, It.IsAny<CancellationToken>()),
            Times.Once);
    }
}