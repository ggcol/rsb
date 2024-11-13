using ASureBus.Abstractions.Configurations;
using ASureBus.ConfigurationObjects;
using ASureBus.Core;
using ASureBus.Core.DI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;

namespace ASureBus.Tests.ASureBus.Core.DI;

[TestFixture]
public class HeavyPropertiesSetupTests
{
    private Mock<IHostBuilder> _mockHostBuilder;
    private Mock<IServiceCollection> _mockServiceCollection;
    private HostBuilderContext _hostBuilderContext;
    private Mock<IConfiguration> _mockConfiguration;

    [SetUp]
    public void SetUp()
    {
        _mockHostBuilder = new Mock<IHostBuilder>();
        _mockServiceCollection = new Mock<IServiceCollection>();
        _mockConfiguration = new Mock<IConfiguration>();
        _hostBuilderContext = new HostBuilderContext(new Dictionary<object, object>())
        {
            Configuration = _mockConfiguration.Object
        };
    }

    [TearDown]
    public void TearDown()
    {
        AsbConfiguration.HeavyProps = null;
        _hostBuilderContext = null!;
    }

    [Test]
    public void UseHeavyProps_WithSettings_RegistersExpectedServices()
    {
        // Arrange
        _mockConfiguration.Setup(c => c.GetSection(It.IsAny<string>()))
            .Returns(new Mock<IConfigurationSection>().Object);

        _mockHostBuilder.Setup(h =>
                h.ConfigureServices(It.IsAny<Action<HostBuilderContext, IServiceCollection>>()))
            .Callback((Action<HostBuilderContext, IServiceCollection> configureServices) =>
            {
                configureServices(_hostBuilderContext, _mockServiceCollection.Object);
            });

        // Act
        _mockHostBuilder.Object.UseHeavyProps<HeavyPropertiesSettings>();

        // Assert
        Assert.That(AsbConfiguration.HeavyProps, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(AsbConfiguration.HeavyProps.DataStorageConnectionString,
                Is.EqualTo("TestConnectionString"));
            Assert.That(AsbConfiguration.HeavyProps.DataStorageContainer,
                Is.EqualTo("TestContainer"));
        });
    }

    [Test]
    public void UseHeavyProps_WithHeavyPropertiesConfig_RegistersExpectedServices()
    {
        // Arrange
        var heavyPropsConfig = new HeavyPropertiesConfig
        {
            DataStorageConnectionString = "TestConnectionString",
            DataStorageContainer = "TestContainer"
        };

        _mockHostBuilder.Setup(h =>
                h.ConfigureServices(It.IsAny<Action<HostBuilderContext, IServiceCollection>>()))
            .Callback((Action<HostBuilderContext, IServiceCollection> configureServices) =>
            {
                configureServices(_hostBuilderContext, _mockServiceCollection.Object);
            });

        // Act
        _mockHostBuilder.Object.UseHeavyProps(heavyPropsConfig);

        // Assert
        Assert.That(AsbConfiguration.HeavyProps, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(AsbConfiguration.HeavyProps.DataStorageConnectionString,
                Is.EqualTo("TestConnectionString"));
            Assert.That(AsbConfiguration.HeavyProps.DataStorageContainer,
                Is.EqualTo("TestContainer"));
        });
    }
}

internal class HeavyPropertiesSettings : IConfigureHeavyProperties
{
    public string? DataStorageConnectionString { get; set; } = "TestConnectionString";
    public string? DataStorageContainer { get; set; } = "TestContainer";
}