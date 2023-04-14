using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Rinzler78.NetExtension.Json.Converters;

public sealed class CollectionConverter<CollectionElement> : JsonConverter<ICollection<CollectionElement>>
{
    public override ICollection<CollectionElement>? Read(ref Utf8JsonReader reader, Type typeToConvert,
        JsonSerializerOptions? options)
    {
        return JsonSerializer.Deserialize<Collection<CollectionElement>>(ref reader, options);
    }

    public override void Write(Utf8JsonWriter writer, ICollection<CollectionElement> value,
        JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(value, options);
    }
}