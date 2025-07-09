using Rinzler78.NetExtension.Tasks;
using Rinzler78.NetExtension.Tests.TestHelpers;

namespace Rinzler78.NetExtension.Tests.Tasks;

public class ReusableTaskTestsSimplified
{
    [Fact]
    public void Constructor_ShouldCreateTaskWithAction()
    {
        // Arrange
        var executed = false;
        Action action = () => executed = true;

        // Act
        var task = new ReusableTask(action);

        // Assert
        task.Should().NotBeNull();
        executed.Should().BeFalse();
    }

    [Fact]
    public void Invoke_ShouldExecuteAction()
    {
        // Arrange
        var executed = false;
        Action action = () => executed = true;
        var task = new ReusableTask(action);

        // Act
        task.InvokeSync();

        // Assert
        executed.Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_ShouldExecuteAction()
    {
        // Arrange
        var executed = false;
        Action action = () => executed = true;
        var task = new ReusableTask(action);

        // Act
        await task.Invoke();

        // Assert
        executed.Should().BeTrue();
    }

    [Fact]
    public void Invoke_ShouldBeReusable()
    {
        // Arrange
        var executionCount = 0;
        Action action = () => executionCount++;
        var task = new ReusableTask(action);

        // Act
        task.InvokeSync();
        task.InvokeSync();
        task.InvokeSync();

        // Assert
        executionCount.Should().Be(3);
    }

    [Fact]
    public void Invoke_ShouldHandleExceptions()
    {
        // Arrange
        Action action = () => throw new InvalidOperationException("Test exception");
        var task = new ReusableTask(action);

        // Act & Assert
        try
        {
            task.InvokeSync();
            Assert.Fail("Should have thrown exception");
        }
        catch (InvalidOperationException ex)
        {
            Assert.Equal("Test exception", ex.Message);
        }
    }

    [Fact]
    public void Invoke_ShouldWorkWithComplexActions()
    {
        // Arrange
        var results = new List<string>();
        Action action = () =>
        {
            var data = TestData.Strings.Simple;
            var processed = data.ToUpper();
            results.Add(processed);
        };
        var task = new ReusableTask(action);

        // Act
        task.InvokeSync();
        task.InvokeSync();

        // Assert
        results.Should().HaveCount(2);
        results.Should().AllBeEquivalentTo("HELLO");
    }
}
