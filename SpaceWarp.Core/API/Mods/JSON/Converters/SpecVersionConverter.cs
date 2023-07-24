using System;
using Newtonsoft.Json;

namespace SpaceWarp.API.Mods.JSON.Converters;

internal class SpecVersionConverter : JsonConverter<SpecVersion>
{
    public override void WriteJson(JsonWriter writer, SpecVersion value, JsonSerializer serializer)
    {
        writer.WriteValue(value.ToString());
    }

    public override SpecVersion ReadJson(
        JsonReader reader,
        Type objectType,
        SpecVersion existingValue,
        bool hasExistingValue,
        JsonSerializer serializer
    )
    {
        var specVersion = (string)reader.Value;
        return new SpecVersion(specVersion);
    }
}