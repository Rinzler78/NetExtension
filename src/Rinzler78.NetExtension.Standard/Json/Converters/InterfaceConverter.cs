using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Rinzler78.NetExtension.Json.Converters
{
    public class InterfaceConverter<Implementation, Interface> : JsonConverter<Interface> where Implementation : class, Interface
    {
        public override Interface Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return JsonSerializer.Deserialize<Implementation>(ref reader, options);
        }

        public override void Write(Utf8JsonWriter writer, Interface value, JsonSerializerOptions options)
        { }
    }

    public class InterfaceConverterFactory<Interface, Implementation> : JsonConverterFactory
        where Implementation : class, Interface, new()

    {
        public readonly Type InterfaceType = typeof(Interface);
        public readonly Type ImplementationType = typeof(Implementation);

        public override bool CanConvert(Type typeToConvert)
        {
            return typeToConvert == InterfaceType;
        }

        public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        {
            var converterType = typeof(InterfaceConverter<,>).MakeGenericType(ImplementationType, InterfaceType);

            return (JsonConverter)Activator.CreateInstance(converterType);
        }
    }
}