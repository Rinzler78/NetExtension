using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Rinzler78.NetExtension.Json.Converters;

public sealed class CollectionInterfaceConverterFactory<CollectionElement> : JsonConverterFactory
{
    public readonly Type CollectionElementType = typeof(CollectionElement);

    public override bool CanConvert(Type typeToConvert)
    {
        if (typeToConvert == typeof(ICollection<>).MakeGenericType(CollectionElementType)
            && typeToConvert.GenericTypeArguments[0] == CollectionElementType)
        {
            return true;
        }

        return false;
    }

    public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        return (JsonConverter?)Activator.CreateInstance(typeof(CollectionConverter<>).MakeGenericType(CollectionElementType));
    }
}