using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Rinzler78.NetExtension.Json.Converters;

public class InterfaceConverterFactory<Interface, Implementation> : JsonConverterFactory
    where Implementation : class, Interface, new()

{
    public readonly Type ImplementationType = typeof(Implementation);
    public readonly Type InterfaceType = typeof(Interface);

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