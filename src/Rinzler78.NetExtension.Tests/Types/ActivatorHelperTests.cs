using System;
using System.Text;
using FluentAssertions;
using Rinzler78.NetExtension.Types;
using Xunit;

namespace Rinzler78.NetExtension.Tests.Types;

/// <summary>
/// Unit tests for <see cref="ActivatorHelper"/>.
/// Covers both the non-generic (<c>CreateInstance(Type, object[])</c>) and
/// generic (<c>CreateInstance&lt;T&gt;(Type, object[])</c>) overloads.
/// </summary>
[Trait("Category", "Unit")]
public class ActivatorHelperTests
{
    // -------------------------------------------------------------------------
    // Helper types used across tests
    // -------------------------------------------------------------------------

    /// <summary>A simple class with a parameterless constructor.</summary>
    private sealed class SimpleType
    {
        public int Value { get; set; }
    }

    /// <summary>A class with a required constructor argument.</summary>
    private sealed class TypeWithRequiredArg
    {
        public string Label { get; }
        public TypeWithRequiredArg(string label) => Label = label;
    }

    /// <summary>A class that derives from a parent class, used for the generic-overload cast test.</summary>
    private class BaseType
    {
        public virtual string Name => "Base";
    }

    private sealed class DerivedType : BaseType
    {
        public override string Name => "Derived";
    }

    // -------------------------------------------------------------------------
    // Non-generic overload: CreateInstance(Type, params object[])
    // -------------------------------------------------------------------------

    [Fact]
    public void CreateInstance_ParameterlessConstructor_ReturnsNewInstance()
    {
        // Act
        var instance = typeof(SimpleType).CreateInstance();

        // Assert
        instance.Should().NotBeNull()
            .And.BeOfType<SimpleType>("Activator must construct the exact requested type");
    }

    [Fact]
    public void CreateInstance_WithMatchingConstructorArgument_ReturnsInitialisedInstance()
    {
        // Act
        var instance = typeof(TypeWithRequiredArg).CreateInstance("hello");

        // Assert
        instance.Should().BeOfType<TypeWithRequiredArg>();
        ((TypeWithRequiredArg)instance!).Label.Should().Be("hello");
    }

    [Fact]
    public void CreateInstance_StringBuilderWithCapacity_ReturnsConfiguredInstance()
    {
        // StringBuilder(int capacity) is a well-known BCL constructor with arguments.
        var instance = typeof(StringBuilder).CreateInstance(64);

        instance.Should().BeOfType<StringBuilder>();
        ((StringBuilder)instance!).Capacity.Should().Be(64);
    }

    /// <summary>
    /// When <paramref name="type"/> is <c>null</c>, <see cref="Activator.CreateInstance"/>
    /// throws <see cref="ArgumentNullException"/>.
    /// </summary>
    [Fact]
    public void CreateInstance_NullType_ThrowsArgumentNullException()
    {
        // Activator.CreateInstance(null, args) → ArgumentNullException.
        var act = () => ((Type)null!).CreateInstance();

        act.Should().Throw<ArgumentNullException>(
            "Activator.CreateInstance does not accept a null type argument");
    }

    /// <summary>
    /// When the supplied arguments do not match any constructor signature,
    /// <see cref="Activator.CreateInstance"/> throws <see cref="MissingMethodException"/>.
    /// </summary>
    [Fact]
    public void CreateInstance_WrongConstructorArguments_ThrowsMissingMethodException()
    {
        // TypeWithRequiredArg only has TypeWithRequiredArg(string).
        // Passing (int, double) matches no overload.
        var act = () => typeof(TypeWithRequiredArg).CreateInstance(42, 3.14);

        act.Should().Throw<MissingMethodException>(
            "Activator cannot find a constructor that matches the supplied argument types");
    }

    [Fact]
    public void CreateInstance_ValueType_ReturnsBoxedDefaultValue()
    {
        // Value types (structs) are supported; Activator returns the default value boxed.
        var instance = typeof(int).CreateInstance();

        instance.Should().Be(0, "Activator.CreateInstance on a value type returns the default");
    }

    // -------------------------------------------------------------------------
    // Generic overload: CreateInstance<T>(Type, params object[])
    // -------------------------------------------------------------------------

    [Fact]
    public void CreateInstance_Generic_ParameterlessConstructor_ReturnsTypedInstance()
    {
        var instance = typeof(SimpleType).CreateInstance<SimpleType>();

        instance.Should().NotBeNull()
            .And.BeOfType<SimpleType>();
    }

    [Fact]
    public void CreateInstance_Generic_WithConstructorArgument_ReturnsInitialisedInstance()
    {
        var instance = typeof(TypeWithRequiredArg).CreateInstance<TypeWithRequiredArg>("world");

        instance.Should().NotBeNull();
        instance!.Label.Should().Be("world");
    }

    [Fact]
    public void CreateInstance_Generic_DerivedTypeReturnedAsBaseType_WorksCorrectly()
    {
        // The generic overload allows casting to a parent/contract type.
        var instance = typeof(DerivedType).CreateInstance<BaseType>();

        instance.Should().NotBeNull()
            .And.BeOfType<DerivedType>("the concrete type is DerivedType");
        instance!.Name.Should().Be("Derived");
    }

    /// <summary>
    /// When the constructed object cannot be cast to the requested <typeparamref name="T"/>,
    /// a <see cref="InvalidCastException"/> is thrown.  This documents the contract: the
    /// caller is responsible for supplying compatible types.
    /// </summary>
    [Fact]
    public void CreateInstance_Generic_IncompatibleType_ThrowsInvalidCastException()
    {
        // StringBuilder cannot be cast to DerivedType.
        var act = () => typeof(StringBuilder).CreateInstance<DerivedType>();

        act.Should().Throw<InvalidCastException>(
            "the generic overload performs an unchecked cast — incompatible types throw");
    }

    [Fact]
    public void CreateInstance_Generic_NullType_ThrowsArgumentNullException()
    {
        var act = () => ((Type)null!).CreateInstance<SimpleType>();

        act.Should().Throw<ArgumentNullException>(
            "Activator.CreateInstance does not accept a null type argument");
    }

    [Fact]
    public void CreateInstance_Generic_WrongConstructorArguments_ThrowsMissingMethodException()
    {
        var act = () => typeof(TypeWithRequiredArg).CreateInstance<TypeWithRequiredArg>(true, 99);

        act.Should().Throw<MissingMethodException>(
            "no constructor on TypeWithRequiredArg accepts (bool, int)");
    }
}
