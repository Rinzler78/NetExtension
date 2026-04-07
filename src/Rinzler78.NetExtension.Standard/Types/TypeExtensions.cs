using System;
using System.Collections.Generic;
using System.Reflection;

namespace Rinzler78.NetExtension.Types;

/// <summary>
/// Provides extension methods for <see cref="Type"/> and <see cref="MemberInfo"/> inspection.
/// </summary>
public static class TypeExtensions
{
    private static readonly HashSet<Type> SimpleTypes = new()
    {
        typeof(string),
        typeof(decimal),
        typeof(DateTime),
        typeof(DateTimeOffset),
        typeof(TimeSpan),
        typeof(Guid)
    };

    /// <summary>
    ///     Determine whether a type is simple (String, Decimal, DateTime, etc)
    ///     or complex (i.e. custom class with public properties and methods).
    /// </summary>
    /// <see href="http://stackoverflow.com/questions/2442534/how-to-test-if-type-is-primitive" />
    public static bool IsSimpleType(
        this Type type)
    {
        return
            type.IsValueType ||
            type.IsPrimitive ||
            SimpleTypes.Contains(type) ||
            Convert.GetTypeCode(type) != TypeCode.Object;
    }

    /// <summary>
    /// Gets the underlying type of a member (event handler type, field type, method return type, or property type).
    /// </summary>
    /// <param name="member">The member to inspect.</param>
    /// <returns>The underlying type of the member.</returns>
    /// <exception cref="ArgumentException">Thrown when the member type is not supported.</exception>
    public static Type? GetUnderlyingType(this MemberInfo member)
    {
        switch (member.MemberType)
        {
            case MemberTypes.Event:
                return ((EventInfo)member).EventHandlerType;

            case MemberTypes.Field:
                return ((FieldInfo)member).FieldType;

            case MemberTypes.Method:
                return ((MethodInfo)member).ReturnType;

            case MemberTypes.Property:
                return ((PropertyInfo)member).PropertyType;

            default:
                throw new ArgumentException
                (
                    "Input MemberInfo must be if type EventInfo, FieldInfo, MethodInfo, or PropertyInfo",
                    nameof(member)
                );
        }
    }
}
