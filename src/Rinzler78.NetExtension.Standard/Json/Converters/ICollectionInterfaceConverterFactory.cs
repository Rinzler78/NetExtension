using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Rinzler78.NetExtension.Json.Converters
{
    public class InterfaceConverter<M, I> : JsonConverter<I> where M : class, I
    {
        public override I Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return JsonSerializer.Deserialize<M>(ref reader, options);
        }

        public override void Write(Utf8JsonWriter writer, I value, JsonSerializerOptions options) { }
    }

    public class CollectionConverter<ElementType> : JsonConverter<ICollection<ElementType>>
    {
        public override ICollection<ElementType> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return JsonSerializer.Deserialize<Collection<ElementType>>(ref reader, options);
        }

        public override void Write(Utf8JsonWriter writer, ICollection<ElementType> value, JsonSerializerOptions options)
        {
            JsonSerializer.Serialize(value, options);
        }
    }

    public class ICollectionInterfaceConverterFactory<Interface> : JsonConverterFactory
    {
        public readonly Type InterfaceType = typeof(Interface);

        public override bool CanConvert(Type typeToConvert)
        {
            if (typeToConvert.Equals(typeof(ICollection<>).MakeGenericType(InterfaceType))
             && typeToConvert.GenericTypeArguments[0].Equals(InterfaceType))
            {
                return true;
            }

            return false;
        }

        public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        {
            return (JsonConverter)Activator.CreateInstance(
                typeof(CollectionConverter<>).MakeGenericType(InterfaceType));
        }
    }

    public class InterfaceConverterFactory<Interface, Implementation> : JsonConverterFactory
        where Implementation : class, Interface, new()

    {
        public readonly Type InterfaceType = typeof(Interface);
        public readonly Type ConcreteType = typeof(Implementation);

        public override bool CanConvert(Type typeToConvert)
        {
            return typeToConvert == InterfaceType;
        }

        public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        {
            var converterType = typeof(InterfaceConverter<,>).MakeGenericType(ConcreteType, InterfaceType);

            return (JsonConverter)Activator.CreateInstance(converterType);
        }
    }
}
