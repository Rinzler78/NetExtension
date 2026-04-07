using System;

namespace Rinzler78.NetExtension.Types;

/// <summary>
/// Provides extension methods for creating instances of types using <see cref="Activator"/>.
/// </summary>
public static class ActivatorHelper
{
    /// <summary>
    /// Creates an instance of the specified type using the provided constructor arguments.
    /// </summary>
    /// <param name="type">The type to instantiate.</param>
    /// <param name="args">Constructor arguments.</param>
    /// <returns>The created instance, or <see langword="null"/> if creation fails.</returns>
    public static object? CreateInstance(this Type type, params object[] args)
    {
        return Activator.CreateInstance(type, args);
    }

    /// <summary>
    /// Creates a strongly-typed instance of the specified type using the provided constructor arguments.
    /// </summary>
    /// <typeparam name="TInstance">The expected return type.</typeparam>
    /// <param name="type">The type to instantiate.</param>
    /// <param name="args">Constructor arguments.</param>
    /// <returns>The created instance cast to <typeparamref name="TInstance"/>.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the instance could not be created.</exception>
    public static TInstance CreateInstance<TInstance>(this Type type, params object[] args)
    {
        var instance = CreateInstance(type, args)
            ?? throw new InvalidOperationException($"Failed to create instance of type '{type.FullName}'.");
        return (TInstance)instance;
    }
}
