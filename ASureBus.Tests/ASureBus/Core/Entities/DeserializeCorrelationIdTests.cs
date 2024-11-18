using ASureBus.Core.Entities;

namespace ASureBus.Tests.ASureBus.Core.Entities;

[TestFixture]
public class DeserializeCorrelationIdTests
{
    [Test]
    public void CorrelationId_ShouldBeSetCorrectly()
    {
        // Arrange
        var correlationId = Guid.NewGuid();
        
        // Act
        var deserializeCorrelationId = new DeserializeCorrelationId
        {
            CorrelationId = correlationId
        };
        
        // Assert
        Assert.That(deserializeCorrelationId.CorrelationId, Is.EqualTo(correlationId));
    }

    [Test]
    public void CorrelationId_ShouldBeEmptyGuid_WhenNotSet()
    {
        //Act
        var deserializeCorrelationId = new DeserializeCorrelationId();
        
        //Asset
        Assert.That(deserializeCorrelationId.CorrelationId, Is.EqualTo(Guid.Empty));
    }
}