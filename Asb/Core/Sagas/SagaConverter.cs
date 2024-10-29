using System.Text.Json;
using System.Text.Json.Serialization;
using Asb.Utils;

namespace Asb.Core.Sagas;

internal class SagaConverter(Type sagaType, Type sagaDataType)
    : JsonConverter<object>
{
    public override bool CanConvert(Type typeToConvert)
    {
        return sagaType.IsAssignableFrom(typeToConvert);
    }

    public override object Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var jsonDocument = JsonDocument.ParseValue(ref reader);
        //TODO hardcoded string
        var sagaDataJson = jsonDocument.RootElement.GetProperty("SagaData").GetRawText();
        var sagaData = Serializer.Deserialize(sagaDataJson, sagaDataType, options);

        var saga = Activator.CreateInstance(sagaType);
        //TODO hardcoded string
        var sagaDataProperty = sagaType.GetProperty("SagaData");
        sagaDataProperty.SetValue(saga, sagaData);

        return saga;
    }

    public override void Write(Utf8JsonWriter writer, object value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WritePropertyName("SagaData");
        var sagaData = value.GetType().GetProperty("SagaData").GetValue(value);
        Serializer.Serialize(writer, sagaData, sagaDataType, options);
        writer.WriteEndObject();
    }
}