using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Rinzler78.NetExtension.Linq;

namespace Rinzler78.NetExtension.Tests.Linq;

public class EnumerableExtensionTests
{
    [Fact]
    public void TryAggregate_WithValidIntegerCollection_ShouldReturnSum()
    {
        // Arrange
        var numbers = new[] { 1, 2, 3, 4, 5 };

        // Act
        var result = numbers.TryAggregate((a, b) => a + b);

        // Assert
        Assert.Equal(15, result);
    }

    [Fact]
    public void TryAggregate_WithValidStringCollection_ShouldReturnConcatenation()
    {
        // Arrange
        var strings = new[] { "Hello", " ", "World", "!" };

        // Act
        var result = strings.TryAggregate((a, b) => a + b);

        // Assert
        Assert.Equal("Hello World!", result);
    }

    [Fact]
    public void TryAggregate_WithSingleElement_ShouldReturnThatElement()
    {
        // Arrange
        var singleElement = new[] { 42 };

        // Act
        var result = singleElement.TryAggregate((a, b) => a + b);

        // Assert
        Assert.Equal(42, result);
    }

    [Fact]
    public void TryAggregate_WithEmptyCollection_ShouldReturnDefault()
    {
        // Arrange
        var emptyCollection = new int[0];

        // Act
        var result = emptyCollection.TryAggregate((a, b) => a + b);

        // Assert
        Assert.Equal(0, result); // default(int) is 0
    }

    [Fact]
    public void TryAggregate_WithNullCollection_ShouldReturnDefault()
    {
        // Arrange
        IEnumerable<int>? nullCollection = null;

        // Act
        var result = nullCollection!.TryAggregate((a, b) => a + b);

        // Assert
        Assert.Equal(0, result); // default(int) is 0
    }

    [Fact]
    public void TryAggregate_WithFunctionThatThrows_ShouldReturnDefault()
    {
        // Arrange
        var numbers = new[] { 1, 2, 3 };
        Func<int, int, int> throwingFunc = (a, b) => throw new InvalidOperationException("Test exception");

        // Act
        var result = numbers.TryAggregate(throwingFunc);

        // Assert
        Assert.Equal(0, result); // default(int) is 0
    }

    [Fact]
    public void TryAggregate_WithComplexObjects_ShouldReturnCorrectResult()
    {
        // Arrange
        var people = new[]
        {
            new { Name = "John", Age = 25 },
            new { Name = "Jane", Age = 30 },
            new { Name = "Bob", Age = 35 }
        };

        // Act - Aggregate ages
        var result = people.Select(p => p.Age).TryAggregate((a, b) => a + b);

        // Assert
        Assert.Equal(90, result);
    }

    [Fact]
    public void TryAggregate_WithDoubleValues_ShouldReturnCorrectResult()
    {
        // Arrange
        var doubles = new[] { 1.5, 2.5, 3.0 };

        // Act
        var result = doubles.TryAggregate((a, b) => a + b);

        // Assert
        Assert.Equal(7.0, result, 10); // 10 decimal places precision
    }

    [Fact]
    public void TryAggregate_WithMaxFunction_ShouldReturnMaxValue()
    {
        // Arrange
        var numbers = new[] { 5, 2, 8, 1, 9, 3 };

        // Act
        var result = numbers.TryAggregate((a, b) => System.Math.Max(a, b));

        // Assert
        Assert.Equal(9, result);
    }

    [Fact]
    public void TryAggregate_WithMinFunction_ShouldReturnMinValue()
    {
        // Arrange
        var numbers = new[] { 5, 2, 8, 1, 9, 3 };

        // Act
        var result = numbers.TryAggregate((a, b) => System.Math.Min(a, b));

        // Assert
        Assert.Equal(1, result);
    }

    [Fact]
    public void TryAggregate_WithReferenceType_EmptyCollection_ShouldReturnNull()
    {
        // Arrange
        var emptyStrings = new string[0];

        // Act
        var result = emptyStrings.TryAggregate((a, b) => a + b);

        // Assert
        Assert.Null(result); // default(string) is null
    }

    [Fact]
    public void TryAggregate_WithLargeCollection_ShouldHandleCorrectly()
    {
        // Arrange
        var largeCollection = Enumerable.Range(1, 1000);

        // Act
        var result = largeCollection.TryAggregate((a, b) => a + b);

        // Assert
        var expectedSum = 1000 * 1001 / 2; // Sum of 1 to 1000
        Assert.Equal(expectedSum, result);
    }
}
