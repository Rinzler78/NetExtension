using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Rinzler78.NetExtension.Observable;
using Xunit;

namespace Rinzler78.NetExtension.Tests.Observable;

/// <summary>
/// Performance tests for ObservableObject dependency management optimization.
/// </summary>
[Trait("Category", "Unit")]
public class ObservableObjectDependencyPerformanceTests
{
    [Theory]
    [InlineData(5)]    // Small collection - should use O(n²) algorithm
    [InlineData(15)]   // Large collection - should use O(n) HashSet algorithm
    [InlineData(50)]   // Very large collection - should use O(n) HashSet algorithm
    public void AttachDependencies_WithVariousCollectionSizes_ShouldWorkCorrectly(int size)
    {
        // Arrange
        var testObj = new TestObservableObjectWithDependencies();
        var dependencies = Enumerable.Range(0, size)
            .Select(i => new TestObservableObjectWithDependencies { Name = $"Dep{i}" })
            .Cast<IObservableObject>()
            .ToArray();

        // Act
        var stopwatch = Stopwatch.StartNew();
        testObj.AttachDependenciesPublic(dependencies);
        stopwatch.Stop();

        // Assert
        Assert.Equal(size, testObj.Dependencies.Count);

        // Verify all dependencies are attached
        foreach (var dep in dependencies)
        {
            Assert.Contains(dep, testObj.Dependencies);
        }

        // Performance assertion - larger collections should complete in reasonable time
        if (size >= 50)
        {
            Assert.True(stopwatch.ElapsedMilliseconds < 100,
                $"AttachDependencies took {stopwatch.ElapsedMilliseconds}ms for {size} items, expected < 100ms");
        }
    }

    [Fact]
    public void AttachDependencies_WithDuplicates_ShouldNotAddDuplicates()
    {
        // Arrange
        var testObj = new TestObservableObjectWithDependencies();
        var dep1 = new TestObservableObjectWithDependencies { Name = "Dep1" };
        var dep2 = new TestObservableObjectWithDependencies { Name = "Dep2" };

        // Pre-attach some dependencies
        testObj.AttachDependenciesPublic(dep1, dep2);

        // Act - try to attach duplicates
        testObj.AttachDependenciesPublic(dep1, dep2, dep1); // dep1 appears twice

        // Assert - should still have only 2 unique dependencies
        Assert.Equal(2, testObj.Dependencies.Count);
        Assert.Contains(dep1, testObj.Dependencies);
        Assert.Contains(dep2, testObj.Dependencies);
    }

    [Fact]
    public void AttachDependencies_PerformanceTest_LargeCollection()
    {
        // Arrange
        const int size = 1000;
        var testObj = new TestObservableObjectWithDependencies();
        var dependencies = Enumerable.Range(0, size)
            .Select(i => new TestObservableObjectWithDependencies { Name = $"Dep{i}" })
            .Cast<IObservableObject>()
            .ToArray();

        // Act
        var stopwatch = Stopwatch.StartNew();
        testObj.AttachDependenciesPublic(dependencies);
        stopwatch.Stop();

        // Assert
        Assert.Equal(size, testObj.Dependencies.Count);

        // Performance assertion - should complete quickly with HashSet optimization
        Assert.True(stopwatch.ElapsedMilliseconds < 50,
            $"AttachDependencies took {stopwatch.ElapsedMilliseconds}ms for {size} items, expected < 50ms with HashSet optimization");
    }

    [Theory]
    [InlineData(5)]    // Small collection - should use O(n²) algorithm
    [InlineData(15)]   // Large collection - should use O(n) HashSet algorithm
    [InlineData(50)]   // Very large collection - should use O(n) HashSet algorithm
    public void DetachDependencies_WithVariousCollectionSizes_ShouldWorkCorrectly(int size)
    {
        // Arrange
        var testObj = new TestObservableObjectWithDependencies();
        var dependencies = Enumerable.Range(0, size)
            .Select(i => new TestObservableObjectWithDependencies { Name = $"Dep{i}" })
            .Cast<IObservableObject>()
            .ToArray();

        // Pre-attach all dependencies
        testObj.AttachDependenciesPublic(dependencies);

        // Select half of the dependencies to detach
        var dependenciesToDetach = dependencies.Take(size / 2).ToArray();

        // Act
        var stopwatch = Stopwatch.StartNew();
        testObj.DetachDependenciesPublic(dependenciesToDetach);
        stopwatch.Stop();

        // Assert
        Assert.Equal(size - dependenciesToDetach.Length, testObj.Dependencies.Count);

        // Verify correct dependencies were detached
        foreach (var dep in dependenciesToDetach)
        {
            Assert.DoesNotContain(dep, testObj.Dependencies);
        }

        // Verify remaining dependencies are still attached
        foreach (var dep in dependencies.Skip(size / 2))
        {
            Assert.Contains(dep, testObj.Dependencies);
        }

        // Performance assertion - larger collections should complete in reasonable time
        if (size >= 50)
        {
            Assert.True(stopwatch.ElapsedMilliseconds < 100,
                $"DetachDependencies took {stopwatch.ElapsedMilliseconds}ms for {size} items, expected < 100ms");
        }
    }

    [Fact]
    public void DetachDependencies_WithNullParameter_ShouldDetachAll()
    {
        // Arrange
        var testObj = new TestObservableObjectWithDependencies();
        var dependencies = Enumerable.Range(0, 10)
            .Select(i => new TestObservableObjectWithDependencies { Name = $"Dep{i}" })
            .Cast<IObservableObject>()
            .ToArray();

        testObj.AttachDependenciesPublic(dependencies);

        // Act
        testObj.DetachDependenciesPublic(null);

        // Assert
        Assert.Empty(testObj.Dependencies);
    }

    [Fact]
    public void DetachDependencies_WithEmptyArray_ShouldDetachAll()
    {
        // Arrange
        var testObj = new TestObservableObjectWithDependencies();
        var dependencies = Enumerable.Range(0, 10)
            .Select(i => new TestObservableObjectWithDependencies { Name = $"Dep{i}" })
            .Cast<IObservableObject>()
            .ToArray();

        testObj.AttachDependenciesPublic(dependencies);

        // Act
        testObj.DetachDependenciesPublic(new IObservableObject[0]);

        // Assert
        Assert.Empty(testObj.Dependencies);
    }

    [Fact]
    public void DetachDependencies_PerformanceTest_LargeCollection()
    {
        // Arrange
        const int size = 1000;
        var testObj = new TestObservableObjectWithDependencies();
        var dependencies = Enumerable.Range(0, size)
            .Select(i => new TestObservableObjectWithDependencies { Name = $"Dep{i}" })
            .Cast<IObservableObject>()
            .ToArray();

        testObj.AttachDependenciesPublic(dependencies);

        // Select half of the dependencies to detach
        var dependenciesToDetach = dependencies.Take(size / 2).ToArray();

        // Act
        var stopwatch = Stopwatch.StartNew();
        testObj.DetachDependenciesPublic(dependenciesToDetach);
        stopwatch.Stop();

        // Assert
        Assert.Equal(size / 2, testObj.Dependencies.Count);

        // Performance assertion - should complete quickly with HashSet optimization
        Assert.True(stopwatch.ElapsedMilliseconds < 50,
            $"DetachDependencies took {stopwatch.ElapsedMilliseconds}ms for {size} items, expected < 50ms with HashSet optimization");
    }

    [Fact]
    public void DetachDependencies_WithNonExistentDependencies_ShouldNotThrow()
    {
        // Arrange
        var testObj = new TestObservableObjectWithDependencies();
        var existingDep = new TestObservableObjectWithDependencies { Name = "Existing" };
        var nonExistentDep = new TestObservableObjectWithDependencies { Name = "NonExistent" };

        testObj.AttachDependenciesPublic(existingDep);

        // Act & Assert
        var exception = Record.Exception(() => testObj.DetachDependenciesPublic(nonExistentDep));
        Assert.Null(exception);

        // Verify existing dependency is still attached
        Assert.Contains(existingDep, testObj.Dependencies);
    }

    /// <summary>
    /// Test class that exposes protected AttachDependencies and DetachDependencies methods for testing.
    /// </summary>
    private class TestObservableObjectWithDependencies : ObservableObject
    {
        private string _name = string.Empty;

        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        /// <summary>
        /// Public wrapper for protected AttachDependencies method.
        /// </summary>
        public void AttachDependenciesPublic(params IObservableObject[] observableObjects)
        {
            AttachDependencies(observableObjects);
        }

        /// <summary>
        /// Public wrapper for protected DetachDependencies method.
        /// </summary>
        public void DetachDependenciesPublic(params IObservableObject[]? observableObjects)
        {
            DetachDependencies(observableObjects!);
        }
    }
}
