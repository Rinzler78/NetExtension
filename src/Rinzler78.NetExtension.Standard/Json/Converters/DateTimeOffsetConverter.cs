using System;
using Newtonsoft.Json.Converters;

namespace Rinzler78.NetExtension.Json.Converters;

public class DateTimeOffsetConverter : IsoDateTimeConverter
{
    public DateTimeOffsetConverter()
    {
        DateTimeStyles = System.Globalization.DateTimeStyles.AssumeUniversal;
    }
}

