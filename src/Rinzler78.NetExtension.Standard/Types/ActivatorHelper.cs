using System;

namespace Rinzler78.NetExtension.Types;

public static class ActivatorHelper
{
    private static object? CreateInstance(this Type type, params object[] args)
    {
        return Activator.CreateInstance(type, args);
    }

    public static InstanceType? CreateInstance<InstanceType>(this Type type, params object[] args)
    {
        return (InstanceType)CreateInstance(type, args)!;
    }
}