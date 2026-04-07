using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Rinzler78.NetExtension.Json.Converters;

/// <summary>
/// System.Text.Json converter factory that creates <see cref="InterfaceConverter{Implementation, Interface}"/> instances for interface-to-implementation mapping.
/// </summary>
/// <typeparam name="Interface">The interface type to convert.</typeparam>
/// <typeparam name="Implementation">The concrete type that implements the interface.</typeparam>
public sealed class InterfaceConverterFactory<Interface, Implementation> : JsonConverterFactory
    where Implementation : class, Interface, new()

{
    /// <summary>
    /// Gets the concrete implementation type used by this factory.
    /// </summary>
    public Type ImplementationType { get; } = typeof(Implementation);

    /// <summary>
    /// Gets the interface type handled by this factory.
    /// </summary>
    public Type InterfaceType { get; } = typeof(Interface);

    /// <summary>
    /// Determines whether this factory can create a converter for the specified type.
    /// </summary>
    /// <param name="typeToConvert">The type to check.</param>
    /// <returns><c>true</c> if the type matches the interface type; otherwise <c>false</c>.</returns>
    public override bool CanConvert(Type typeToConvert)
    {
        return typeToConvert == InterfaceType;
    }

    /// <summary>
    /// Creates an <see cref="InterfaceConverter{Implementation, Interface}"/> for the specified type.
    /// </summary>
    /// <param name="typeToConvert">The type to create a converter for.</param>
    /// <param name="options">The serializer options.</param>
    /// <returns>A new converter instance.</returns>
    public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        var converterType = typeof(InterfaceConverter<,>).MakeGenericType(ImplementationType, InterfaceType);

        return (JsonConverter?)Activator.CreateInstance(converterType);
    }
}
