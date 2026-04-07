# ⚡ Task Utilities

[![Async](https://img.shields.io/badge/Async-Await-purple?style=flat-square)](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/concepts/async/)
[![Threading](https://img.shields.io/badge/Threading-Safe-green?style=flat-square)](https://docs.microsoft.com/en-us/dotnet/standard/threading/)

Task management utilities for async operations and cancellation handling.

## 📋 Table of Contents

- [Overview](#overview)
- [Core Classes](#core-classes)
- [Task Extensions](#task-extensions)
- [`WhenAll<TResult>` — Generic Overload](#whenalltresult--generic-overload)
- [ReusableTask Pattern](#reusable-task-pattern)

## Overview

The Task utilities in Rinzler78.NetExtension provide enhanced async/await capabilities and simplified cancellation token management.

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
Single-flight async wrapper around a synchronous `Action` with cancellation support:

```csharp
public sealed class ReusableTask : IDisposable
{
    public ReusableTask(Action action)
    public Task Invoke()        // Async invocation (deduplicated — returns existing Task if running)
    public void InvokeSync()    // Synchronous invocation on calling thread
    public bool Cancel()        // Request cancellation of current invocation
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

## ReusableTask Pattern

`ReusableTask` wraps a synchronous `Action` into a single-flight async task with built-in cancellation. Concurrent calls to `Invoke()` return the same in-flight `Task` instead of starting a new one.

```csharp
public class ReusableTaskExample : IDisposable
{
    private readonly ReusableTask _reusableTask;

    public ReusableTaskExample()
    {
        _reusableTask = new ReusableTask(() => ProcessData());
    }

    public async Task ExecuteAsync()
    {
        // Deduplicated: concurrent calls return the same Task
        await _reusableTask.Invoke();
    }

    public void Stop()
    {
        _reusableTask.Cancel();
    }

    public void Dispose()
    {
        _reusableTask.Dispose();
    }

    private void ProcessData()
    {
        // Synchronous work wrapped by ReusableTask
    }
}
```

---

[← Back to Main Documentation](../README.md)
