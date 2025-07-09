using System.Collections.Concurrent;
using System.ComponentModel;
using Rinzler78.NetExtension.Tests.TestHelpers;

namespace Rinzler78.NetExtension.Tests.Observable;

public class ObservableObjectTests
{
    [Fact]
    public void SetProperty_ShouldUpdateValue_WhenValueIsDifferent()
    {
        // Arrange
        var testObj = new TestObservableObject();
        const string newValue = "test";

        // Act
        testObj.Name = newValue;

        // Assert
        testObj.Name.Should().Be(newValue);
    }

    [Fact]
    public void SetProperty_ShouldNotUpdateValue_WhenValueIsSame()
    {
        // Arrange
        var testObj = new TestObservableObject { Name = "test" };
        var originalValue = testObj.Name;

        // Act
        testObj.Name = "test";

        // Assert
        testObj.Name.Should().Be(originalValue);
    }

    [Fact]
    public void SetProperty_ShouldRaisePropertyChangedEvent_WhenValueChanges()
    {
        // Arrange
        var testObj = new TestObservableObject();
        var eventRaised = false;
        string? propertyName = null;

        testObj.PropertyChanged += (sender, e) =>
        {
            eventRaised = true;
            propertyName = e.PropertyName;
        };

        // Act
        testObj.Name = "test";

        // Assert
        eventRaised.Should().BeTrue();
        propertyName.Should().Be(nameof(TestObservableObject.Name));
    }

    [Fact]
    public void SetProperty_ShouldNotRaisePropertyChangedEvent_WhenValueDoesNotChange()
    {
        // Arrange
        var testObj = new TestObservableObject { Name = "test" };
        var eventRaised = false;

        testObj.PropertyChanged += (sender, e) => eventRaised = true;

        // Act
        testObj.Name = "test";

        // Assert
        eventRaised.Should().BeFalse();
    }

    [Fact]
    public void SetProperty_ShouldBeThreadSafe_WhenCalledConcurrently()
    {
        // Arrange
        var testObj = new TestObservableObject();
        const int threadCount = 10;
        const int iterationsPerThread = 100;
        var exceptions = new ConcurrentBag<Exception>();

        // Act
        var tasks = new List<Task>();
        for (int i = 0; i < threadCount; i++)
        {
            var threadId = i;
            tasks.Add(Task.Run(() =>
            {
                try
                {
                    for (int j = 0; j < iterationsPerThread; j++)
                    {
                        testObj.Name = $"Thread{threadId}_Iteration{j}";
                        testObj.Value = threadId * 1000 + j;
                    }
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                }
            }));
        }

        Task.WaitAll(tasks.ToArray());

        // Assert
        exceptions.Should().BeEmpty();
        testObj.Name.Should().NotBeNullOrEmpty();
        testObj.Value.Should().BeGreaterOrEqualTo(0);
    }

    [Fact]
    public void PropertyChanged_ShouldBeRaisedForDependentProperties()
    {
        // Arrange
        var testObj = new TestObservableObjectWithDependencies();
        var propertyChangedEvents = new List<string>();

        testObj.PropertyChanged += (sender, e) =>
        {
            if (e.PropertyName != null)
                propertyChangedEvents.Add(e.PropertyName);
        };

        // Act
        testObj.FirstName = "John";
        testObj.LastName = "Doe";

        // Assert
        propertyChangedEvents.Should().Contain(nameof(TestObservableObjectWithDependencies.FirstName));
        propertyChangedEvents.Should().Contain(nameof(TestObservableObjectWithDependencies.LastName));
    }

    [Fact]
    public void SetProperty_ShouldHandleNullValues()
    {
        // Arrange
        var testObj = new TestObservableObject();

        // Act & Assert
        testObj.Invoking(x => x.Name = null!).Should().NotThrow();
        testObj.Name.Should().BeNull();
    }

    [Fact]
    public void SetProperty_ShouldHandleValueTypes()
    {
        // Arrange
        var testObj = new TestObservableObject();
        var eventRaised = false;

        testObj.PropertyChanged += (sender, e) =>
        {
            if (e.PropertyName == nameof(TestObservableObject.Value))
                eventRaised = true;
        };

        // Act
        testObj.Value = 42;

        // Assert
        testObj.Value.Should().Be(42);
        eventRaised.Should().BeTrue();
    }

    [Fact]
    public void SetProperty_ShouldHandleBooleanValues()
    {
        // Arrange
        var testObj = new TestObservableObject();
        var eventRaised = false;

        testObj.PropertyChanged += (sender, e) =>
        {
            if (e.PropertyName == nameof(TestObservableObject.IsActive))
                eventRaised = true;
        };

        // Act
        testObj.IsActive = true;

        // Assert
        testObj.IsActive.Should().BeTrue();
        eventRaised.Should().BeTrue();
    }

    [Fact]
    public void PropertyChanged_ShouldProvideCorrectSender()
    {
        // Arrange
        var testObj = new TestObservableObject();
        object? sender = null;

        testObj.PropertyChanged += (s, e) => sender = s;

        // Act
        testObj.Name = "test";

        // Assert
        sender.Should().Be(testObj);
    }

    [Fact]
    public void MultiplePropertyChanges_ShouldRaiseEventsInCorrectOrder()
    {
        // Arrange
        var testObj = new TestObservableObject();
        var eventOrder = new List<string>();

        testObj.PropertyChanged += (sender, e) =>
        {
            if (e.PropertyName != null)
                eventOrder.Add(e.PropertyName);
        };

        // Act
        testObj.Name = "test";
        testObj.Value = 42;
        testObj.IsActive = true;

        // Assert
        eventOrder.Should().ContainInOrder(
            nameof(TestObservableObject.Name),
            nameof(TestObservableObject.Value),
            nameof(TestObservableObject.IsActive)
        );
    }

    [Fact]
    public void SetProperty_ShouldHandleRapidSuccessiveChanges()
    {
        // Arrange
        var testObj = new TestObservableObject();
        var eventCount = 0;

        testObj.PropertyChanged += (sender, e) =>
        {
            if (e.PropertyName == nameof(TestObservableObject.Value))
                eventCount++;
        };

        // Act
        for (int i = 1; i <= 1000; i++)
        {
            testObj.Value = i;
        }

        // Assert
        testObj.Value.Should().Be(1000);
        eventCount.Should().Be(1000);
    }

    // Note: ObservableObject dispose tests removed as Dispose method is protected
}
