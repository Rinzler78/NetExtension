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

    /// <summary>
    /// Copies property values from source object to target object.
    /// </summary>
    /// <typeparam name="Tu">The type of the target object.</typeparam>
    /// <param name="source">The source object to copy from.</param>
    /// <param name="target">The target object to copy to.</param>
    /// <param name="onCopyToFailedForProperty">Callback invoked when property copy fails.</param>
    /// <exception cref="ArgumentNullException">Thrown when source is null.</exception>
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
                    catch (ArgumentException)
                    {
                        // Property type mismatch or invalid value
                        onCopyToFailedForProperty?.Invoke(
                            new ValueTuple<PropertyInfo, object?>(sourceProperty, source),
                            new ValueTuple<PropertyInfo, object?>(targetPropertyInfo, target));
                    }
                    catch (TargetException)
                    {
                        // Target object doesn't match the property's reflected type
                        onCopyToFailedForProperty?.Invoke(
                            new ValueTuple<PropertyInfo, object?>(sourceProperty, source),
                            new ValueTuple<PropertyInfo, object?>(targetPropertyInfo, target));
                    }
                    catch (TargetInvocationException)
                    {
                        // Property setter threw an exception
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

    /// <summary>
    /// Gets the value of a property from an object.
    /// </summary>
    /// <typeparam name="ReturnType">The expected return type.</typeparam>
    /// <param name="src">The source object.</param>
    /// <param name="propName">The name of the property to get.</param>
    /// <returns>The property value cast to the specified type, or default if not found or cast fails.</returns>
    /// <exception cref="ArgumentNullException">Thrown when src or propName is null.</exception>
    public static ReturnType? GetPropertyValue<ReturnType>(this object src, string propName)
    {
        ArgumentNullException.ThrowIfNull(src);
        ArgumentNullException.ThrowIfNull(propName);

        var value = src.GetType().GetProperty(propName)?.GetValue(src, null);
        if (value is ReturnType t)
            return t;
        return default;
    }
}
