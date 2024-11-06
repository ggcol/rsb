using ASureBus.Abstractions.Configurations;
using ASureBus.Abstractions.Configurations.ConfigObjects;
using ASureBus.Accessories.Heavy;
using ASureBus.Core.DI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;

namespace ASureBus.Tests.Core.DI;

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
        _mockServiceCollection.Verify(
            s => s.Add(It.Is<ServiceDescriptor>(d => d.ServiceType == typeof(IHeavyIO))),
            Times.Once);
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
        _mockServiceCollection.Verify(
            s => s.Add(It.Is<ServiceDescriptor>(d => d.ServiceType == typeof(IHeavyIO))),
            Times.Once);
    }
}

internal class HeavyPropertiesSettings : IConfigureHeavyProperties
{
    public string? DataStorageConnectionString { get; set; }
    public string? DataStorageContainer { get; set; }
}