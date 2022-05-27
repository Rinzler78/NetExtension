using System;

namespace Rinzler78.NetExtension.Types;

public static class ConvertHelper
{
    public static T ConvertValue<T>(this object value)
    {
        return (T)Convert.ChangeType(value, typeof(T));
    }
}