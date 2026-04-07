using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Rinzler78.NetExtension.Json.Converters;

/// <summary>
/// System.Text.Json converter that deserializes an interface type by delegating to a concrete implementation type.
/// </summary>
/// <typeparam name="Implementation">The concrete type that implements the interface.</typeparam>
/// <typeparam name="Interface">The interface type to convert.</typeparam>
public sealed class InterfaceConverter<Implementation, Interface> : JsonConverter<Interface>
    where Implementation : class, Interface
{
    /// <summary>
    /// Reads JSON and deserializes it as the concrete <typeparamref name="Implementation"/> type.
    /// </summary>
    /// <param name="reader">The Utf8JsonReader to read from.</param>
    /// <param name="typeToConvert">The target type to convert.</param>
    /// <param name="options">The serializer options.</param>
    /// <returns>The deserialized implementation instance, or null.</returns>
    public override Interface? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions? options)
    {
        return JsonSerializer.Deserialize<Implementation>(ref reader, options);
    }

    /// <summary>
    /// Writes the interface value to JSON by serializing it as the concrete <typeparamref name="Implementation"/> type.
    /// </summary>
    /// <param name="writer">The Utf8JsonWriter to write to.</param>
    /// <param name="value">The interface value to serialize.</param>
    /// <param name="options">The serializer options.</param>
    public override void Write(Utf8JsonWriter writer, Interface value, JsonSerializerOptions options)
        => JsonSerializer.Serialize(writer, (Implementation?)value, options);
}
