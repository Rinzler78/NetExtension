# ⚡ Task Utilities

[![Async](https://img.shields.io/badge/Async-Await-purple?style=flat-square)](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/concepts/async/)
[![Threading](https://img.shields.io/badge/Threading-Safe-green?style=flat-square)](https://docs.microsoft.com/en-us/dotnet/standard/threading/)

Advanced task management utilities for async operations, cancellation handling, and parallel processing optimizations.

## 📋 Table of Contents

- [Overview](#overview)
- [Core Classes](#core-classes)
- [Task Extensions](#task-extensions)
- [`WhenAll<TResult>` — Generic Overload](#whenalltresult--generic-overload)
- [Cancellation Utilities](#cancellation-utilities)
- [Parallel Operations](#parallel-operations)
- [Usage Examples](#usage-examples)
- [Advanced Patterns](#advanced-patterns)
- [Performance Optimization](#performance-optimization)
- [Best Practices](#best-practices)

## Overview

The Task utilities in Rinzler78.NetExtension provide enhanced async/await capabilities, simplified cancellation token management, and optimized parallel processing patterns for high-performance applications.

## Core Classes

### `TaskHelper`
Static utility class providing extension methods for Task operations:

```csharp
public static class TaskHelper
{
    // Cancellation token utilities
    public static bool IsCancellationRequested(this CancellationTokenSource cancellationTokenSource)

    // Task state checking
    public static bool IsRunning(this Task task)

    // Parallel execution
    public static Task WhenAll(this IEnumerable<Task> tasks)
}
```

### `ReusableTask`
Optimized task implementation for scenarios requiring frequent task creation:

```csharp
public class ReusableTask : IDisposable
{
    // Implementation details for reusable task patterns
    public Task StartAsync(Func<CancellationToken, Task> taskFactory, CancellationToken cancellationToken = default)
    public void Reset()
    public void Dispose()
}
```

## Task Extensions

### Task State Checking
```csharp
var task = SomeAsyncOperation();

// Check if task is still running
bool isRunning = task.IsRunning();
if (isRunning)
{
    Console.WriteLine("Task is still executing...");
}

// Wait for completion
await task;
Console.WriteLine("Task completed");
```

### Cancellation Token Utilities
```csharp
using var cts = new CancellationTokenSource();

// Extension method for checking cancellation
bool isCancelled = cts.IsCancellationRequested();
if (isCancelled)
{
    Console.WriteLine("Operation was cancelled");
    return;
}

// Set timeout
cts.CancelAfter(TimeSpan.FromSeconds(30));

// Use in async operations
await SomeAsyncOperation(cts.Token);
```

### Parallel Task Execution
```csharp
var tasks = new List<Task>
{
    ProcessDataAsync(data1),
    ProcessDataAsync(data2),
    ProcessDataAsync(data3)
};

// Extension method for Task.WhenAll
await tasks.WhenAll();
Console.WriteLine("All tasks completed");
```

### `WhenAll<TResult>` — Generic overload

```csharp
public static Task<TResult[]> WhenAll<TResult>(this IEnumerable<Task<TResult>> tasks)
```

Creates a task that completes when all provided tasks have completed, returning an array of their results in the original order.

**Example:**

```csharp
var urls = new[]
{
    "https://api.example.com/items/1",
    "https://api.example.com/items/2",
    "https://api.example.com/items/3",
};

// Fetch all concurrently and collect results
string[] responses = await urls
    .Select(url => url.HttpGetStringAsync())
    .WhenAll();

// Or with typed results
ItemDto[] items = await itemIds
    .Select(id => GetItemAsync(id))
    .WhenAll();
```

## Cancellation Utilities

### Safe Cancellation Checking
```csharp
public static class SafeCancellationExtensions
{
    public static bool IsSafelyCancellationRequested(this CancellationTokenSource? cts)
    {
        return cts?.IsCancellationRequested() ?? true;
    }

    public static void SafeCancel(this CancellationTokenSource? cts)
    {
        if (cts != null && !cts.IsCancellationRequested())
        {
            cts.Cancel();
        }
    }
}

// Usage
CancellationTokenSource? cts = GetCancellationTokenSource();
if (cts.IsSafelyCancellationRequested())
{
    // Handle cancellation
    return;
}
```

### Timeout Management
```csharp
public static class TimeoutExtensions
{
    public static async Task<T> WithTimeout<T>(this Task<T> task, TimeSpan timeout)
    {
        using var cts = new CancellationTokenSource(timeout);

        try
        {
            return await task;
        }
        catch (OperationCanceledException) when (cts.Token.IsCancellationRequested)
        {
            throw new TimeoutException($"Operation timed out after {timeout}");
        }
    }

    public static async Task WithTimeout(this Task task, TimeSpan timeout)
    {
        using var cts = new CancellationTokenSource(timeout);

        try
        {
            await task;
        }
        catch (OperationCanceledException) when (cts.Token.IsCancellationRequested)
        {
            throw new TimeoutException($"Operation timed out after {timeout}");
        }
    }
}

// Usage
var result = await SomeAsyncOperation().WithTimeout(TimeSpan.FromSeconds(30));
```

## Parallel Operations

### Parallel Processing with Cancellation
```csharp
public static class ParallelTaskProcessor
{
    public static async Task ProcessInParallel<T>(
        IEnumerable<T> items,
        Func<T, CancellationToken, Task> processor,
        CancellationToken cancellationToken = default,
        int maxConcurrency = Environment.ProcessorCount)
    {
        using var semaphore = new SemaphoreSlim(maxConcurrency);

        var tasks = items.Select(async item =>
        {
            await semaphore.WaitAsync(cancellationToken);
            try
            {
                await processor(item, cancellationToken);
            }
            finally
            {
                semaphore.Release();
            }
        });

        await tasks.WhenAll();
    }
}

// Usage
var urls = new[] { "url1", "url2", "url3", "url4", "url5" };
using var cts = new CancellationTokenSource();

await ParallelTaskProcessor.ProcessInParallel(
    urls,
    async (url, ct) => await DownloadAsync(url, ct),
    cts.Token,
    maxConcurrency: 3
);
```

### Batch Processing
```csharp
public static class BatchProcessor
{
    public static async Task<List<TResult>> ProcessInBatches<TItem, TResult>(
        IEnumerable<TItem> items,
        Func<TItem, CancellationToken, Task<TResult>> processor,
        int batchSize = 10,
        CancellationToken cancellationToken = default)
    {
        var results = new List<TResult>();
        var batches = items.Chunk(batchSize);

        foreach (var batch in batches)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var batchTasks = batch.Select(item => processor(item, cancellationToken));
            var batchResults = await batchTasks.WhenAll();

            results.AddRange(batchResults);
        }

        return results;
    }
}

// Usage
var items = Enumerable.Range(1, 100).ToList();
var results = await BatchProcessor.ProcessInBatches(
    items,
    async (item, ct) => await ProcessItemAsync(item, ct),
    batchSize: 10
);
```

## Usage Examples

### Background Task Management
```csharp
public class BackgroundTaskManager
{
    private readonly List<Task> _backgroundTasks = new();
    private readonly CancellationTokenSource _cts = new();

    public void StartBackgroundTask(Func<CancellationToken, Task> taskFactory)
    {
        var task = Task.Run(() => taskFactory(_cts.Token));
        _backgroundTasks.Add(task);
    }

    public async Task StopAllAsync()
    {
        _cts.Cancel();

        // Wait for all tasks to complete or timeout
        var runningTasks = _backgroundTasks.Where(t => t.IsRunning()).ToList();

        if (runningTasks.Any())
        {
            await runningTasks.WhenAll().WithTimeout(TimeSpan.FromSeconds(30));
        }
    }

    public void Dispose()
    {
        _cts.Dispose();
    }
}

// Usage
var manager = new BackgroundTaskManager();

manager.StartBackgroundTask(async ct =>
{
    while (!ct.IsCancellationRequested)
    {
        await DoBackgroundWork(ct);
        await Task.Delay(1000, ct);
    }
});

// Later...
await manager.StopAllAsync();
```

### Retry Logic with Cancellation
```csharp
public static class RetryHelper
{
    public static async Task<T> RetryAsync<T>(
        Func<CancellationToken, Task<T>> operation,
        int maxAttempts = 3,
        TimeSpan delay = default,
        CancellationToken cancellationToken = default)
    {
        if (delay == default)
            delay = TimeSpan.FromSeconds(1);

        for (int attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                return await operation(cancellationToken);
            }
            catch (Exception ex) when (attempt < maxAttempts)
            {
                Console.WriteLine($"Attempt {attempt} failed: {ex.Message}");
                await Task.Delay(delay * attempt, cancellationToken); // Exponential backoff
            }
        }

        // Final attempt
        return await operation(cancellationToken);
    }
}

// Usage
var result = await RetryHelper.RetryAsync(
    async ct => await UnreliableOperation(ct),
    maxAttempts: 3,
    delay: TimeSpan.FromSeconds(2)
);
```

### Task Coordination
```csharp
public class TaskCoordinator
{
    private readonly List<Task> _tasks = new();
    private readonly object _lock = new();

    public void AddTask(Task task)
    {
        lock (_lock)
        {
            _tasks.Add(task);
        }
    }

    public async Task WaitForAnyCompletion()
    {
        List<Task> currentTasks;
        lock (_lock)
        {
            currentTasks = _tasks.Where(t => t.IsRunning()).ToList();
        }

        if (currentTasks.Any())
        {
            await Task.WhenAny(currentTasks);
        }
    }

    public async Task WaitForAllCompletion()
    {
        List<Task> currentTasks;
        lock (_lock)
        {
            currentTasks = _tasks.ToList();
        }

        await currentTasks.WhenAll();
    }

    public void RemoveCompletedTasks()
    {
        lock (_lock)
        {
            _tasks.RemoveAll(t => !t.IsRunning());
        }
    }
}
```

## Advanced Patterns

### Reusable Task Implementation
```csharp
public class ReusableTaskExample
{
    private readonly ReusableTask _reusableTask = new();

    public async Task ExecuteOperationAsync(string data, CancellationToken cancellationToken = default)
    {
        await _reusableTask.StartAsync(async ct =>
        {
            // Reusable task logic
            await ProcessDataAsync(data, ct);
        }, cancellationToken);
    }

    public void Reset()
    {
        _reusableTask.Reset();
    }

    public void Dispose()
    {
        _reusableTask.Dispose();
    }
}
```

### Task Result Caching
```csharp
public class TaskResultCache<TKey, TResult>
{
    private readonly Dictionary<TKey, Task<TResult>> _cache = new();
    private readonly object _lock = new();

    public Task<TResult> GetOrCreateAsync(TKey key, Func<TKey, CancellationToken, Task<TResult>> factory,
        CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            if (_cache.TryGetValue(key, out var existingTask) && existingTask.IsRunning())
            {
                return existingTask;
            }

            var newTask = factory(key, cancellationToken);
            _cache[key] = newTask;

            // Clean up completed tasks
            newTask.ContinueWith(t =>
            {
                lock (_lock)
                {
                    if (_cache.TryGetValue(key, out var cachedTask) && cachedTask == t)
                    {
                        _cache.Remove(key);
                    }
                }
            }, TaskScheduler.Default);

            return newTask;
        }
    }
}

// Usage
var cache = new TaskResultCache<string, string>();
var result = await cache.GetOrCreateAsync("key1", async (key, ct) => await FetchDataAsync(key, ct));
```

### Producer-Consumer Pattern
```csharp
public class ProducerConsumer<T>
{
    private readonly Channel<T> _channel = Channel.CreateUnbounded<T>();
    private readonly List<Task> _consumerTasks = new();
    private readonly CancellationTokenSource _cts = new();

    public void StartConsumers(int consumerCount, Func<T, CancellationToken, Task> processor)
    {
        for (int i = 0; i < consumerCount; i++)
        {
            var task = Task.Run(async () =>
            {
                await foreach (var item in _channel.Reader.ReadAllAsync(_cts.Token))
                {
                    await processor(item, _cts.Token);
                }
            });

            _consumerTasks.Add(task);
        }
    }

    public async Task ProduceAsync(T item)
    {
        await _channel.Writer.WriteAsync(item);
    }

    public void CompleteProduction()
    {
        _channel.Writer.Complete();
    }

    public async Task StopAsync()
    {
        _cts.Cancel();
        await _consumerTasks.WhenAll();
    }
}
```

## Performance Optimization

### Task Pool Management
```csharp
public static class TaskPoolManager
{
    private static readonly ObjectPool<Task> TaskPool = new DefaultObjectPool<Task>(
        new TaskPoolPolicy(), Environment.ProcessorCount * 2);

    public static Task GetTask(Func<Task> factory)
    {
        var task = TaskPool.Get();

        // Configure task with factory
        task.ContinueWith(t => TaskPool.Return(t), TaskScheduler.Default);

        return task;
    }
}
```

### Memory-Efficient Async Operations
```csharp
public static class MemoryEfficientAsync
{
    public static async IAsyncEnumerable<TResult> ProcessStreamAsync<TInput, TResult>(
        IAsyncEnumerable<TInput> source,
        Func<TInput, CancellationToken, ValueTask<TResult>> processor,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await foreach (var item in source.WithCancellation(cancellationToken))
        {
            yield return await processor(item, cancellationToken);
        }
    }
}
```

## Best Practices

### 1. Proper Cancellation Handling
```csharp
public static async Task<T> SafeAsyncOperation<T>(
    Func<CancellationToken, Task<T>> operation,
    CancellationToken cancellationToken = default)
{
    try
    {
        return await operation(cancellationToken);
    }
    catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
    {
        // Expected cancellation
        throw;
    }
    catch (Exception ex)
    {
        // Log and handle unexpected exceptions
        Console.WriteLine($"Unexpected error: {ex.Message}");
        throw;
    }
}
```

### 2. Resource Management
```csharp
public static async Task<T> WithResourceManagement<T>(
    Func<CancellationToken, Task<T>> operation,
    CancellationToken cancellationToken = default)
{
    using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

    try
    {
        return await operation(cts.Token);
    }
    finally
    {
        // Ensure cleanup
        cts.Cancel();
    }
}
```

### 3. Error Handling
```csharp
public static async Task<Result<T>> SafeExecuteAsync<T>(
    Func<CancellationToken, Task<T>> operation,
    CancellationToken cancellationToken = default)
{
    try
    {
        var result = await operation(cancellationToken);
        return Result<T>.Success(result);
    }
    catch (OperationCanceledException)
    {
        return Result<T>.Cancelled();
    }
    catch (Exception ex)
    {
        return Result<T>.Failure(ex);
    }
}
```

### 4. Performance Monitoring
```csharp
public static async Task<T> WithPerformanceMonitoring<T>(
    Func<CancellationToken, Task<T>> operation,
    string operationName,
    CancellationToken cancellationToken = default)
{
    var stopwatch = Stopwatch.StartNew();

    try
    {
        var result = await operation(cancellationToken);
        Console.WriteLine($"{operationName} completed in {stopwatch.ElapsedMilliseconds}ms");
        return result;
    }
    catch (Exception ex)
    {
        Console.WriteLine($"{operationName} failed after {stopwatch.ElapsedMilliseconds}ms: {ex.Message}");
        throw;
    }
}
```

---

[← Back to Main Documentation](../README.md)
