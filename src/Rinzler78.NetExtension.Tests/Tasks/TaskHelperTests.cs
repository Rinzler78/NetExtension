using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Rinzler78.NetExtension.Tasks;

namespace Rinzler78.NetExtension.Tests.Tasks;

[Trait("Category", "Unit")]
public class TaskHelperTests
{
    [Fact]
    public void IsCancellationRequested_WithNullToken_ShouldReturnTrue()
    {
        CancellationTokenSource? nullSource = null;

        var result = nullSource!.IsCancellationRequested();

        result.Should().BeTrue();
    }

    [Fact]
    public void IsCancellationRequested_WithNonCancelledToken_ShouldReturnFalse()
    {
        using var source = new CancellationTokenSource();

        var result = source.IsCancellationRequested();

        result.Should().BeFalse();
    }

    [Fact]
    public void IsCancellationRequested_WithCancelledToken_ShouldReturnTrue()
    {
        using var source = new CancellationTokenSource();
        source.Cancel();

        var result = source.IsCancellationRequested();

        result.Should().BeTrue();
    }

    [Fact]
    public void IsRunning_WithNullTask_ShouldReturnFalse()
    {
        Task? nullTask = null;

        var result = nullTask!.IsRunning();

        result.Should().BeFalse();
    }

    [Fact]
    public void IsRunning_WithCompletedTask_ShouldReturnFalse()
    {
        var task = Task.CompletedTask;

        var result = task.IsRunning();

        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsRunning_WithRunningTask_ShouldReturnTrue()
    {
        var tcs = new TaskCompletionSource<bool>();
        var task = tcs.Task;

        var result = task.IsRunning();

        // Complete the task to clean up
        tcs.SetResult(true);
        await task;

        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsRunning_WithDelayedTask_ShouldReturnTrueInitiallyThenFalse()
    {
        var task = Task.Delay(100);

        var runningBefore = task.IsRunning();
        await task;
        var runningAfter = task.IsRunning();

        runningBefore.Should().BeTrue();
        runningAfter.Should().BeFalse();
    }

    [Fact]
    public void IsRunning_WithFaultedTask_ShouldReturnFalse()
    {
        var task = Task.FromException(new InvalidOperationException("Test exception"));

        var result = task.IsRunning();

        result.Should().BeFalse();
    }

    [Fact]
    public void IsRunning_WithCancelledTask_ShouldReturnFalse()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();
        var task = Task.FromCanceled(cts.Token);

        var result = task.IsRunning();

        result.Should().BeFalse();
    }

    [Fact]
    public async Task WhenAll_WithMultipleTasks_ShouldWaitForAll()
    {
        var tasks = new List<Task>
        {
            Task.Delay(50),
            Task.Delay(75),
            Task.Delay(25)
        };

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        await tasks.WhenAll();
        stopwatch.Stop();

        // Should wait for the longest task (75 ms). Upper bound is generous for CI/parallel load.
        stopwatch.ElapsedMilliseconds.Should().BeInRange(40, 5000);
    }

    [Fact]
    public async Task WhenAll_WithEmptyCollection_ShouldCompleteImmediately()
    {
        var tasks = new List<Task>();

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        await tasks.WhenAll();
        stopwatch.Stop();

        // An empty WhenAll should return essentially instantly (well under 1 second).
        stopwatch.Elapsed.Should().BeLessThan(TimeSpan.FromSeconds(1),
            "WhenAll on an empty collection should complete without delay");
    }

    [Fact]
    public async Task WhenAll_WithSingleTask_ShouldWaitForThatTask()
    {
        var delay = TimeSpan.FromMilliseconds(100);
        var tasks = new List<Task> { Task.Delay(delay) };
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        await tasks.WhenAll();
        stopwatch.Stop();

        // CI runners can be heavily contended, so only assert that the awaited
        // task does not complete too early and stays within a broad upper bound.
        stopwatch.ElapsedMilliseconds.Should().BeInRange(
            (long)delay.TotalMilliseconds - 10,
            5000);
    }

    [Fact]
    public async Task WhenAll_WithTaskThatThrows_ShouldPropagateException()
    {
        var tasks = new List<Task>
        {
            Task.Delay(10),
            Task.FromException(new InvalidOperationException("Test exception")),
            Task.Delay(10)
        };

        var act = async () => await tasks.WhenAll();

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Test exception");
    }

    [Fact]
    public async Task WhenAll_WithGenericTasks_ShouldReturnResults()
    {
        var tasks = new List<Task<int>>
        {
            Task.FromResult(1),
            Task.FromResult(2),
            Task.FromResult(3)
        };

        // Using built-in WhenAll for Task<T> (extension only covers non-generic Task).
        var results = await Task.WhenAll(tasks);

        results.Should().Equal(1, 2, 3);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task WhenAll_WithGenericTasks_ShouldReturnResultsInOrder()
    {
        // Arrange
        var tasks = new[]
        {
            Task.FromResult(1),
            Task.FromResult(2),
            Task.FromResult(3),
        };

        // Act
        int[] results = await tasks.WhenAll();

        // Assert
        results.Should().Equal(1, 2, 3);
    }
}
