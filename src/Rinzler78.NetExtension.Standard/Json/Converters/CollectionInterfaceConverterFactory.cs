using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Rinzler78.NetExtension.Json.Converters;

/// <summary>
/// System.Text.Json converter factory that creates <see cref="CollectionConverter{T}"/> instances for <see cref="ICollection{T}"/> types.
/// </summary>
/// <typeparam name="CollectionElement">The element type of the collection.</typeparam>
public sealed class CollectionInterfaceConverterFactory<CollectionElement> : JsonConverterFactory
{
    /// <summary>
    /// Gets the element type of the collection this factory handles.
    /// </summary>
    public Type CollectionElementType { get; } = typeof(CollectionElement);

    /// <summary>
    /// Determines whether this factory can create a converter for the specified type.
    /// </summary>
    /// <param name="typeToConvert">The type to check.</param>
    /// <returns><c>true</c> if the type is <see cref="ICollection{CollectionElement}"/>; otherwise <c>false</c>.</returns>
    public override bool CanConvert(Type typeToConvert)
    {
        if (typeToConvert == typeof(ICollection<>).MakeGenericType(CollectionElementType)
            && typeToConvert.GenericTypeArguments[0] == CollectionElementType)
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// Creates a <see cref="CollectionConverter{T}"/> for the specified collection type.
    /// </summary>
    /// <param name="typeToConvert">The type to create a converter for.</param>
    /// <param name="options">The serializer options.</param>
    /// <returns>A new <see cref="CollectionConverter{T}"/> instance.</returns>
    public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        return (JsonConverter?)Activator.CreateInstance(typeof(CollectionConverter<>).MakeGenericType(CollectionElementType));
    }
}
