using ASureBus.Configurations.ConfigObjects;
using ASureBus.Core;

namespace ASureBus.Tests.Core;

[TestFixture]
public class RsbConfigurationTests
{
    [SetUp]
    public void SetUp()
    {
        RsbConfiguration.ServiceBus = new ServiceBusConfig();
        RsbConfiguration.Cache = new RsbCacheConfig();
        RsbConfiguration.HeavyProps = null;
        RsbConfiguration.DataStorageSagaPersistence = null;
        RsbConfiguration.SqlServerSagaPersistence = null;
    }

    [TearDown]
    public void TearDown()
    {
        RsbConfiguration.ServiceBus = null;
        RsbConfiguration.Cache = null;
        RsbConfiguration.HeavyProps = null;
        RsbConfiguration.DataStorageSagaPersistence = null;
        RsbConfiguration.SqlServerSagaPersistence = null;
    }

    [Test]
    public void UseHeavyProperties_WhenHeavyPropsIsNull_ReturnsFalse()
    {
        // Act
        var result = RsbConfiguration.UseHeavyProperties;

        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public void UseHeavyProperties_WhenHeavyPropsIsNotNull_ReturnsTrue()
    {
        // Arrange
        RsbConfiguration.HeavyProps = new HeavyPropertiesConfig();

        // Act
        var result = RsbConfiguration.UseHeavyProperties;

        // Assert
        Assert.That(result, Is.True);
    }

    [Test]
    public void OffloadSagas_WhenNoSagaPersistenceConfigured_ReturnsFalse()
    {
        // Act
        var result = RsbConfiguration.OffloadSagas;

        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public void OffloadSagas_WhenDataStorageSagaPersistenceConfigured_ReturnsTrue()
    {
        // Arrange
        RsbConfiguration.DataStorageSagaPersistence = new DataStorageSagaPersistenceConfig();

        // Act
        var result = RsbConfiguration.OffloadSagas;

        // Assert
        Assert.That(result, Is.True);
    }

    [Test]
    public void OffloadSagas_WhenSqlServerSagaPersistenceConfigured_ReturnsTrue()
    {
        // Arrange
        RsbConfiguration.SqlServerSagaPersistence = new SqlServerSagaPersistenceConfig();

        // Act
        var result = RsbConfiguration.OffloadSagas;

        // Assert
        Assert.That(result, Is.True);
    }
}