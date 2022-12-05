using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Rinzler78.NetExtension.Json.Converters;

public sealed class InterfaceConverterFactory<Interface, Implementation> : JsonConverterFactory
    where Implementation : class, Interface, new()

{
    public readonly Type _implementationType = typeof(Implementation);
    public readonly Type _interfaceType = typeof(Interface);

    public override bool CanConvert(Type typeToConvert)
    {
        return typeToConvert == _interfaceType;
    }

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        var converterType = typeof(InterfaceConverter<,>).MakeGenericType(_implementationType, _interfaceType);

        return (JsonConverter)Activator.CreateInstance(converterType);
    }
}