using System.Buffers;
using System.Text;
using System.Text.Json;
using ASureBus.Core.Sagas;

namespace ASureBus.Tests.Core.Sagas;

public class SagaData
{
    public string? Data { get; set; }
}

public class Saga
{
    public SagaData? SagaData { get; set; }
}

[TestFixture]
public class SagaConverterTests
{
    private SagaConverter _sagaConverter;
    private JsonSerializerOptions _options;

    [SetUp]
    public void SetUp()
    {
        _sagaConverter = new SagaConverter(typeof(Saga), typeof(SagaData));
        _options = new JsonSerializerOptions
        {
            Converters = { _sagaConverter }
        };
    }

    [Test]
    public void CanConvert_ReturnsTrueForAssignableType()
    {
        // Act
        var result = _sagaConverter.CanConvert(typeof(Saga));

        // Assert
        Assert.IsTrue(result);
    }

    [Test]
    public void CanConvert_ReturnsFalseForNonAssignableType()
    {
        // Act
        var result = _sagaConverter.CanConvert(typeof(string));

        // Assert
        Assert.IsFalse(result);
    }

    [Test]
    public void Read_DeserializesJsonToObject()
    {
        // Arrange
        var json = "{\"SagaData\":{\"Data\":\"TestData\"}}";
        var reader = new Utf8JsonReader(Encoding.UTF8.GetBytes(json));

        // Act
        var result = _sagaConverter.Read(ref reader, typeof(Saga), _options);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.InstanceOf<Saga>());
            Assert.That(((Saga)result).SagaData?.Data, Is.EqualTo("TestData"));
        });
    }

    [Test]
    public void Write_SerializesObjectToJson()
    {
        // Arrange
        var saga = new Saga { SagaData = new SagaData { Data = "TestData" } };
        var buffer = new ArrayBufferWriter<byte>();
        var writer = new Utf8JsonWriter(buffer);

        // Act
        _sagaConverter.Write(writer, saga, _options);
        writer.Flush();
        var json = Encoding.UTF8.GetString(buffer.WrittenMemory.ToArray());

        // Assert
        Assert.That(json, Is.EqualTo("{\"SagaData\":{\"Data\":\"TestData\"}}"));
    }
}