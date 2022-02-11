using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Rinzler78.NetExtension.Json.Converters
{
    public class CollectionConverter<CollectionElement> : JsonConverter<ICollection<CollectionElement>>
    {
        public override ICollection<CollectionElement> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return JsonSerializer.Deserialize<Collection<CollectionElement>>(ref reader, options);
        }

        public override void Write(Utf8JsonWriter writer, ICollection<CollectionElement> value, JsonSerializerOptions options)
        {
            JsonSerializer.Serialize(value, options);
        }
    }

    public class ICollectionInterfaceConverterFactory<CollectionElement> : JsonConverterFactory
    {
        public readonly Type CollectionElementType = typeof(CollectionElement);

        public override bool CanConvert(Type typeToConvert)
        {
            if (typeToConvert.Equals(typeof(ICollection<>).MakeGenericType(CollectionElementType))
             && typeToConvert.GenericTypeArguments[0].Equals(CollectionElementType))
            {
                return true;
            }

            return false;
        }

        public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        {
            return (JsonConverter)Activator.CreateInstance(
                typeof(CollectionConverter<>).MakeGenericType(CollectionElementType));
        }
    }
}