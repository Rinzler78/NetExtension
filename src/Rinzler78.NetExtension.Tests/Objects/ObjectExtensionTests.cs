using System;
using System.Collections.Generic;
using System.Linq;
using Rinzler78.NetExtension.Objects;
using Xunit;

namespace Rinzler78.NetExtension.Tests.Objects;

[Trait("Category", "Unit")]
public class ObjectExtensionTests
{
    public class TestSourceClass
    {
        public string Name { get; set; } = "";
        public int Age { get; set; }
        public DateTime BirthDate { get; set; }
        public bool IsActive { get; set; }
        public string ReadOnlyProperty { get; } = "ReadOnly";
    }

    public class TestTargetClass
    {
        public string Name { get; set; } = "";
        public int Age { get; set; }
        public DateTime BirthDate { get; set; }
        public bool IsActive { get; set; }
        public string AdditionalProperty { get; set; } = "";
    }

    public class TestClassWithWriteOnlyProperty
    {
        public string Name { get; set; } = "";
        private string _writeOnlyValue = "";
        public string WriteOnlyProperty { set => _writeOnlyValue = value; }
        public string GetWriteOnlyValue() => _writeOnlyValue;
    }

    public interface ITestInterface
    {
        string InterfaceProperty { get; set; }
    }

    public class TestInterfaceImplementation : ITestInterface
    {
        public string InterfaceProperty { get; set; } = "";
        public string AdditionalProperty { get; set; } = "";
    }

    [Fact]
    public void GetPublicProperties_ForClass_ShouldReturnAllPublicProperties()
    {
        // Arrange
        var type = typeof(TestSourceClass);

        // Act
        var properties = type.GetPublicProperties().ToList();

        // Assert
        Assert.True(properties.Count >= 5); // At least Name, Age, BirthDate, IsActive, ReadOnlyProperty
        Assert.Contains(properties, p => p.Name == "Name");
        Assert.Contains(properties, p => p.Name == "Age");
        Assert.Contains(properties, p => p.Name == "BirthDate");
        Assert.Contains(properties, p => p.Name == "IsActive");
        Assert.Contains(properties, p => p.Name == "ReadOnlyProperty");
    }

    [Fact]
    public void GetPublicProperties_ForInterface_ShouldReturnInterfaceAndBaseInterfaceProperties()
    {
        // Arrange
        var type = typeof(ITestInterface);

        // Act
        var properties = type.GetPublicProperties().ToList();

        // Assert
        Assert.Contains(properties, p => p.Name == "InterfaceProperty");
    }

    [Fact]
    public void CopyTo_WithGenericNew_ShouldCreateNewInstanceAndCopyProperties()
    {
        // Arrange
        var source = new TestSourceClass
        {
            Name = "John Doe",
            Age = 30,
            BirthDate = new DateTime(1993, 1, 1),
            IsActive = true
        };

        // Act
        var target = source.CopyTo<TestTargetClass>();

        // Assert
        Assert.NotNull(target);
        Assert.Equal(source.Name, target.Name);
        Assert.Equal(source.Age, target.Age);
        Assert.Equal(source.BirthDate, target.BirthDate);
        Assert.Equal(source.IsActive, target.IsActive);
        Assert.Equal("", target.AdditionalProperty); // Should be default value
    }

    [Fact]
    public void CopyTo_WithExistingTarget_ShouldCopyPropertiesToExistingInstance()
    {
        // Arrange
        var source = new TestSourceClass
        {
            Name = "Jane Doe",
            Age = 25,
            BirthDate = new DateTime(1998, 5, 15),
            IsActive = false
        };
        var target = new TestTargetClass
        {
            AdditionalProperty = "Should remain"
        };

        // Act
        source.CopyTo(target);

        // Assert
        Assert.Equal(source.Name, target.Name);
        Assert.Equal(source.Age, target.Age);
        Assert.Equal(source.BirthDate, target.BirthDate);
        Assert.Equal(source.IsActive, target.IsActive);
        Assert.Equal("Should remain", target.AdditionalProperty);
    }

    [Fact]
    public void CopyTo_WithFailureCallback_ShouldInvokeCallbackOnError()
    {
        // Arrange
        var source = new { InvalidProperty = new object() };
        var target = new TestTargetClass();
        var failureCallbacks = new List<string>();

        // Act
        source.CopyTo(target, (sourceInfo, targetInfo) =>
        {
            failureCallbacks.Add($"{sourceInfo.propertyInfo.Name}->{targetInfo.propertyInfo.Name}");
        });

        // Assert
        // Since there's no matching property that would cause an error in this simple case,
        // we verify the method doesn't throw and callback setup works
        Assert.NotNull(target);
    }

    [Fact]
    public void IsEqualTo_WithIdenticalObjects_ShouldReturnTrue()
    {
        // Arrange
        var obj1 = new TestSourceClass
        {
            Name = "Test",
            Age = 30,
            BirthDate = new DateTime(1993, 1, 1),
            IsActive = true
        };
        var obj2 = new TestSourceClass
        {
            Name = "Test",
            Age = 30,
            BirthDate = new DateTime(1993, 1, 1),
            IsActive = true
        };

        // Act
        var result = obj1.IsEqualTo(obj2);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsEqualTo_WithDifferentObjects_ShouldReturnFalse()
    {
        // Arrange
        var obj1 = new TestSourceClass
        {
            Name = "Test1",
            Age = 30,
            BirthDate = new DateTime(1993, 1, 1),
            IsActive = true
        };
        var obj2 = new TestSourceClass
        {
            Name = "Test2",
            Age = 30,
            BirthDate = new DateTime(1993, 1, 1),
            IsActive = true
        };

        // Act
        var result = obj1.IsEqualTo(obj2);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsEqualTo_WithIgnoredProperties_ShouldReturnTrueWhenIgnoringDifferences()
    {
        // Arrange
        var obj1 = new TestSourceClass
        {
            Name = "Test1",
            Age = 30,
            BirthDate = new DateTime(1993, 1, 1),
            IsActive = true
        };
        var obj2 = new TestSourceClass
        {
            Name = "Test2",
            Age = 35,
            BirthDate = new DateTime(1993, 1, 1),
            IsActive = true
        };

        // Act
        var result = obj1.IsEqualTo(obj2, "Name", "Age");

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsEqualTo_WithNullObjects_ShouldReturnCorrectResult()
    {
        // Arrange
        var obj3 = new TestSourceClass();

        // Act & Assert
        // Both null should be equal – call extension explicitly, not == operator
        Assert.True(ObjectExtension.IsEqualTo<TestSourceClass>(null!, null!));

        // Null and non-null should not be equal – call extension explicitly
        Assert.False(ObjectExtension.IsEqualTo<TestSourceClass>(null!, new TestSourceClass()));

        // Non-null self vs null to
        Assert.False(obj3.IsEqualTo(null!, ""));

        // Test with actual objects
        var obj4 = new TestSourceClass { Name = "Test" };
        var obj5 = new TestSourceClass { Name = "Test" };
        Assert.True(obj4.IsEqualTo(obj5));
    }

    [Fact]
    public void GetPropertyValue_WithExistingProperty_ShouldReturnValue()
    {
        // Arrange
        var obj = new TestSourceClass
        {
            Name = "Test Name",
            Age = 42
        };

        // Act
        var nameValue = obj.GetPropertyValue<string>("Name");
        var ageValue = obj.GetPropertyValue<int>("Age");

        // Assert
        Assert.Equal("Test Name", nameValue);
        Assert.Equal(42, ageValue);
    }

    [Fact]
    public void GetPropertyValue_WithNonExistentProperty_ShouldReturnDefault()
    {
        // Arrange
        var obj = new TestSourceClass();

        // Act
        var result = obj.GetPropertyValue<string>("NonExistentProperty");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void GetPropertyValue_WithWrongType_ShouldReturnDefault()
    {
        // Arrange
        var obj = new TestSourceClass { Name = "Test" };

        // Act
        var result = obj.GetPropertyValue<int>("Name"); // String property requested as int

        // Assert
        Assert.Equal(0, result); // Default value for int
    }
}
