using System.Text.Json;
using System.Text.Json.Serialization;

namespace Rsb.Services.StorageAccount;

public class SagaConverter : JsonConverter<object>
{
    private readonly Type _sagaType;
    private readonly Type _sagaDataType;

    public SagaConverter(Type sagaType, Type sagaDataType)
    {
        _sagaType = sagaType;
        _sagaDataType = sagaDataType;
    }

    public override bool CanConvert(Type typeToConvert)
    {
        return _sagaType.IsAssignableFrom(typeToConvert);
    }

    public override object Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var jsonDocument = JsonDocument.ParseValue(ref reader);
        var sagaDataJson = jsonDocument.RootElement.GetProperty("SagaData").GetRawText();
        var sagaData = JsonSerializer.Deserialize(sagaDataJson, _sagaDataType, options);

        var saga = Activator.CreateInstance(_sagaType);
        var sagaDataProperty = _sagaType.GetProperty("SagaData");
        sagaDataProperty.SetValue(saga, sagaData);

        return saga;
    }

    public override void Write(Utf8JsonWriter writer, object value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WritePropertyName("SagaData");
        var sagaData = value.GetType().GetProperty("SagaData").GetValue(value);
        JsonSerializer.Serialize(writer, sagaData, _sagaDataType, options);
        writer.WriteEndObject();
    }
}