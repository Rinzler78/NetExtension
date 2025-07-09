using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Rinzler78.NetExtension.Types;

namespace Rinzler78.NetExtension.Objects;

public static class ObjectExtension
{
    public delegate void OnCopyToFailedForPropertyDelegate(
        (PropertyInfo propertyInfo, object? obj) source,
        (PropertyInfo propertyInfo, object? obj) target);

    public static IEnumerable<PropertyInfo> GetPublicProperties(this Type type)
    {
        if (!type.IsInterface)
            return type.GetProperties();

        return new[] { type }
            .Concat(type.GetInterfaces())
            .SelectMany(i => i.GetProperties());
    }

    public static Tu CopyTo<Tu>(this object source,
        OnCopyToFailedForPropertyDelegate? getValueOnFailedCopyDelegate = null)
        where Tu : new()
    {
        var dest = new Tu();
        source.CopyTo(dest, getValueOnFailedCopyDelegate);
        return dest;
    }

    public static void CopyTo<Tu>(this object source, Tu? target, OnCopyToFailedForPropertyDelegate? onCopyToFailedForProperty = null)
    {
        var sourceProperties = source.GetType().GetPublicProperties().Where(x => x.CanRead).ToList();
        var targetProperties = typeof(Tu).GetPublicProperties()
            .Where(x => x.CanWrite)
            .ToList();

        foreach (var sourceProperty in sourceProperties)
        {
            if (targetProperties.Any(x => string.Equals(x.Name, sourceProperty.Name, StringComparison.Ordinal)))
            {
                var targetPropertyInfo = targetProperties.First(x =>
                    string.Equals(x.Name, sourceProperty.Name, StringComparison.Ordinal));
                //if (targetPropertyInfo.CanWrite && sourceProperty.PropertyType == targetPropertyInfo.PropertyType)
                if (targetPropertyInfo.CanWrite)
                {
                    try
                    {
                        targetPropertyInfo.SetValue(target, sourceProperty.GetValue(source, null), null);
                    }
                    catch (Exception)
                    {
                        onCopyToFailedForProperty?.Invoke(
                            new ValueTuple<PropertyInfo, object?>(sourceProperty, source),
                            new ValueTuple<PropertyInfo, object?>(targetPropertyInfo, target));
                    }
                }
            }
        }
    }

    public static bool IsEqualTo<T>(this T self, T to, params string[] ignore) where T : class
    {
        if (self is not null && to is not null)
        {
            var type = typeof(T);
            var ignoreList = new List<string>(ignore);
            var unequalProperties =
                from pi in type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                where !ignoreList.Contains(pi.Name, StringComparer.Ordinal) && (pi.GetUnderlyingType()?.IsSimpleType() ?? false) &&
                    pi.GetIndexParameters().Length == 0
                let selfValue = type.GetProperty(pi.Name)?.GetValue(self, null)
                let toValue = type.GetProperty(pi.Name)?.GetValue(to, null)
                where selfValue != toValue && selfValue?.Equals(toValue) != true
                select selfValue;
            return !unequalProperties.Any();
        }

        return self == to;
    }

    public static ReturnType? GetPropertyValue<ReturnType>(this object src, string propName)
    {
        var value = src.GetType().GetProperty(propName)?.GetValue(src, null);
        if (value is ReturnType t)
            return t;
        return default;
    }
}
