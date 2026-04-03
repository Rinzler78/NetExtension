using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Rinzler78.NetExtension.Tasks;

/// <summary>
/// Provides utility methods for working with tasks, cancellation tokens, and asynchronous operations.
/// </summary>
public static class TaskHelper
{
    /// <summary>
    /// Determines whether cancellation has been requested for the specified CancellationTokenSource.
    /// Returns true if the source is null, providing safe null-checking behavior.
    /// </summary>
    /// <param name="cancellationTokenSource">The CancellationTokenSource to check</param>
    /// <returns>True if cancellation was requested or if the source is null, false otherwise</returns>
    /// <example>
    /// <code>
    /// var cts = new CancellationTokenSource();
    /// bool isCancelled = cts.IsCancellationRequested(); // false
    /// cts.Cancel();
    /// bool isCancelled2 = cts.IsCancellationRequested(); // true
    ///
    /// CancellationTokenSource nullCts = null;
    /// bool isNull = nullCts.IsCancellationRequested(); // true (safe null handling)
    /// </code>
    /// </example>
    public static bool IsCancellationRequested(this CancellationTokenSource cancellationTokenSource)
    {
        return cancellationTokenSource?.IsCancellationRequested ?? true;
    }

    /// <summary>
    /// Determines whether the specified task is currently running (not completed).
    /// Returns false if the task is null, providing safe null-checking behavior.
    /// </summary>
    /// <param name="task">The task to check</param>
    /// <returns>True if the task is running (not completed), false if the task is completed or null</returns>
    /// <example>
    /// <code>
    /// var task = Task.Run(() => Thread.Sleep(1000));
    /// bool running = task.IsRunning(); // true (while task is executing)
    /// task.Wait();
    /// bool completed = task.IsRunning(); // false (task completed)
    ///
    /// Task nullTask = null;
    /// bool isNull = nullTask.IsRunning(); // false (safe null handling)
    /// </code>
    /// </example>
    public static bool IsRunning(this Task task)
    {
        return !(task?.IsCompleted ?? true);
    }

    /// <summary>
    /// Creates a task that will complete when all of the provided tasks have completed.
    /// This is an extension method wrapper around Task.WhenAll for improved fluent syntax.
    /// </summary>
    /// <param name="tasks">The enumerable collection of tasks to wait for</param>
    /// <returns>A task that represents the completion of all the provided tasks</returns>
    /// <example>
    /// <code>
    /// var tasks = new List&lt;Task&gt;
    /// {
    ///     Task.Delay(100),
    ///     Task.Delay(200),
    ///     Task.Delay(300)
    /// };
    ///
    /// await tasks.WhenAll(); // Waits for all tasks to complete
    /// </code>
    /// </example>
    public static Task WhenAll(this IEnumerable<Task> tasks)
    {
        return Task.WhenAll(tasks);
    }

    /// <summary>
    /// Creates a task that completes when all the provided tasks have completed,
    /// returning an array of their results.
    /// This is an extension method wrapper around <see cref="Task.WhenAll{TResult}(IEnumerable{Task{TResult}})"/>.
    /// </summary>
    /// <typeparam name="TResult">The type of result produced by each task.</typeparam>
    /// <param name="tasks">The collection of tasks to wait for.</param>
    /// <returns>
    /// A task that represents the completion of all provided tasks,
    /// with an array containing each task's result in the same order.
    /// </returns>
    /// <example>
    /// <code>
    /// var urls = new[] { "https://a.example.com", "https://b.example.com" };
    /// string[] responses = await urls
    ///     .Select(url => url.HttpGetStringAsync())
    ///     .WhenAll();
    /// </code>
    /// </example>
    public static Task<TResult[]> WhenAll<TResult>(this IEnumerable<Task<TResult>> tasks)
    {
        return Task.WhenAll(tasks);
    }
}
