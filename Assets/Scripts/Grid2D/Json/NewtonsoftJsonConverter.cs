#nullable enable
using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class NewtonsoftJsonConverter<T> : JsonConverter
{
    public override bool CanConvert(Type objectType) => typeof(T).IsAssignableFrom(objectType);

    public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    {
        var jo = JObject.Load(reader);
        var typeName = jo["Type"]?.Value<string>();
        if (typeName == null || !TypeRegistry.TryGetValue(typeName, out var type))
            throw new JsonSerializationException($"Unknown type: {typeName}");
        return jo.ToObject(type, serializer);
    }

    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        var jo = JObject.FromObject(value!, serializer);
        jo.AddFirst(new JProperty("Type", value!.GetType().Name));
        jo.WriteTo(writer);
    }
}