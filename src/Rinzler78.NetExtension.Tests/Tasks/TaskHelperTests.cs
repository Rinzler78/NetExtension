using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Rinzler78.NetExtension.Tasks;
using Xunit;

namespace Rinzler78.NetExtension.Tests.Tasks;

public class TaskHelperTests
{
    [Fact]
    public void IsCancellationRequested_WithNullToken_ShouldReturnTrue()
    {
        // Arrange
        CancellationTokenSource? nullSource = null;

        // Act
        var result = nullSource!.IsCancellationRequested();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsCancellationRequested_WithNonCancelledToken_ShouldReturnFalse()
    {
        // Arrange
        using var source = new CancellationTokenSource();

        // Act
        var result = source.IsCancellationRequested();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsCancellationRequested_WithCancelledToken_ShouldReturnTrue()
    {
        // Arrange
        using var source = new CancellationTokenSource();
        source.Cancel();

        // Act
        var result = source.IsCancellationRequested();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsRunning_WithNullTask_ShouldReturnFalse()
    {
        // Arrange
        Task? nullTask = null;

        // Act
        var result = nullTask!.IsRunning();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsRunning_WithCompletedTask_ShouldReturnFalse()
    {
        // Arrange
        var task = Task.CompletedTask;

        // Act
        var result = task.IsRunning();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task IsRunning_WithRunningTask_ShouldReturnTrue()
    {
        // Arrange
        var tcs = new TaskCompletionSource<bool>();
        var task = tcs.Task;

        // Act
        var result = task.IsRunning();

        // Complete the task to clean up
        tcs.SetResult(true);
        await task;

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task IsRunning_WithDelayedTask_ShouldReturnTrueInitiallyThenFalse()
    {
        // Arrange
        var task = Task.Delay(100);

        // Act
        var runningBefore = task.IsRunning();
        await task;
        var runningAfter = task.IsRunning();

        // Assert
        Assert.True(runningBefore);
        Assert.False(runningAfter);
    }

    [Fact]
    public void IsRunning_WithFaultedTask_ShouldReturnFalse()
    {
        // Arrange
        var task = Task.FromException(new InvalidOperationException("Test exception"));

        // Act
        var result = task.IsRunning();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsRunning_WithCancelledTask_ShouldReturnFalse()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        cts.Cancel();
        var task = Task.FromCanceled(cts.Token);

        // Act
        var result = task.IsRunning();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task WhenAll_WithMultipleTasks_ShouldWaitForAll()
    {
        // Arrange
        var tasks = new List<Task>
        {
            Task.Delay(50),
            Task.Delay(75),
            Task.Delay(25)
        };

        var startTime = DateTime.Now;

        // Act
        await tasks.WhenAll();
        var endTime = DateTime.Now;

        // Assert
        var elapsed = endTime - startTime;
        Assert.True(elapsed.TotalMilliseconds >= 70); // Should wait for the longest task (75ms)
        Assert.True(elapsed.TotalMilliseconds < 150); // But not too much longer
    }

    [Fact]
    public async Task WhenAll_WithEmptyCollection_ShouldCompleteImmediately()
    {
        // Arrange
        var tasks = new List<Task>();

        // Act
        await tasks.WhenAll();

        // Assert
        // If we reach here, the method completed successfully
        Assert.True(true);
    }

    [Fact]
    public async Task WhenAll_WithSingleTask_ShouldWaitForThatTask()
    {
        // Arrange
        var tasks = new List<Task> { Task.Delay(50) };
        var startTime = DateTime.Now;

        // Act
        await tasks.WhenAll();
        var endTime = DateTime.Now;

        // Assert
        var elapsed = endTime - startTime;
        Assert.True(elapsed.TotalMilliseconds >= 45); // Should wait for the task
        Assert.True(elapsed.TotalMilliseconds < 100);
    }

    [Fact]
    public async Task WhenAll_WithTaskThatThrows_ShouldPropagateException()
    {
        // Arrange
        var tasks = new List<Task>
        {
            Task.Delay(10),
            Task.FromException(new InvalidOperationException("Test exception")),
            Task.Delay(10)
        };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => tasks.WhenAll());
    }

    [Fact]
    public async Task WhenAll_WithGenericTasks_ShouldReturnResults()
    {
        // Arrange
        var tasks = new List<Task<int>>
        {
            Task.FromResult(1),
            Task.FromResult(2),
            Task.FromResult(3)
        };

        // Act
        var results = await Task.WhenAll(tasks); // Using built-in WhenAll for Task<T>

        // Assert
        Assert.Equal(new[] { 1, 2, 3 }, results);
    }
}
