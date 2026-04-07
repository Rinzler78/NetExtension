using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Rinzler78.NetExtension.Json.Converters;

/// <summary>
/// System.Text.Json converter that handles serialization of <see cref="ICollection{T}"/> by delegating to a concrete <see cref="Collection{T}"/>.
/// </summary>
/// <typeparam name="CollectionElement">The element type of the collection.</typeparam>
public sealed class CollectionConverter<CollectionElement> : JsonConverter<ICollection<CollectionElement>>
{
    /// <summary>
    /// Reads JSON and deserializes it into an <see cref="ICollection{CollectionElement}"/>.
    /// </summary>
    /// <param name="reader">The Utf8JsonReader to read from.</param>
    /// <param name="typeToConvert">The target type to convert.</param>
    /// <param name="options">The serializer options.</param>
    /// <returns>The deserialized collection, or null.</returns>
    public override ICollection<CollectionElement>? Read(ref Utf8JsonReader reader, Type typeToConvert,
        JsonSerializerOptions? options)
    {
        return JsonSerializer.Deserialize<Collection<CollectionElement>>(ref reader, options);
    }

    /// <summary>
    /// Writes an <see cref="ICollection{CollectionElement}"/> as a JSON array.
    /// </summary>
    /// <param name="writer">The Utf8JsonWriter to write to.</param>
    /// <param name="value">The collection to serialize.</param>
    /// <param name="options">The serializer options.</param>
    public override void Write(Utf8JsonWriter writer, ICollection<CollectionElement> value,
        JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value.ToArray(), options);
    }
}
