﻿using ASureBus.Abstractions.Configurations.ConfigObjects;
using ASureBus.Core;

namespace ASureBus.Tests.Core;

[TestFixture]
public class AsbConfigurationTests
{
    [SetUp]
    public void SetUp()
    {
        AsbConfiguration.ServiceBus = new ServiceBusConfig();
        AsbConfiguration.Cache = new AsbCacheConfig();
        AsbConfiguration.HeavyProps = null;
        AsbConfiguration.DataStorageSagaPersistence = null;
        AsbConfiguration.SqlServerSagaPersistence = null;
    }

    [TearDown]
    public void TearDown()
    {
        AsbConfiguration.ServiceBus = null;
        AsbConfiguration.Cache = null;
        AsbConfiguration.HeavyProps = null;
        AsbConfiguration.DataStorageSagaPersistence = null;
        AsbConfiguration.SqlServerSagaPersistence = null;
    }

    [Test]
    public void UseHeavyProperties_WhenHeavyPropsIsNull_ReturnsFalse()
    {
        // Act
        var result = AsbConfiguration.UseHeavyProperties;

        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public void UseHeavyProperties_WhenHeavyPropsIsNotNull_ReturnsTrue()
    {
        // Arrange
        AsbConfiguration.HeavyProps = new HeavyPropertiesConfig();

        // Act
        var result = AsbConfiguration.UseHeavyProperties;

        // Assert
        Assert.That(result, Is.True);
    }

    [Test]
    public void OffloadSagas_WhenNoSagaPersistenceConfigured_ReturnsFalse()
    {
        // Act
        var result = AsbConfiguration.OffloadSagas;

        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public void OffloadSagas_WhenDataStorageSagaPersistenceConfigured_ReturnsTrue()
    {
        // Arrange
        AsbConfiguration.DataStorageSagaPersistence = new DataStorageSagaPersistenceConfig();

        // Act
        var result = AsbConfiguration.OffloadSagas;

        // Assert
        Assert.That(result, Is.True);
    }

    [Test]
    public void OffloadSagas_WhenSqlServerSagaPersistenceConfigured_ReturnsTrue()
    {
        // Arrange
        AsbConfiguration.SqlServerSagaPersistence = new SqlServerSagaPersistenceConfig();

        // Act
        var result = AsbConfiguration.OffloadSagas;

        // Assert
        Assert.That(result, Is.True);
    }
}