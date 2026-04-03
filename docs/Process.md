# 🔧 Process Utilities

[![System.Diagnostics](https://img.shields.io/badge/System-Diagnostics-blue?style=flat-square)](https://docs.microsoft.com/en-us/dotnet/api/system.diagnostics.process)
[![Cross Platform](https://img.shields.io/badge/Cross-Platform-green?style=flat-square)](https://docs.microsoft.com/en-us/dotnet/core/rid-catalog)

Advanced process execution utilities for running external commands, capturing output, and managing system processes with async support.

## 📋 Table of Contents

- [Overview](#overview)
- [Core Classes](#core-classes)
- [Basic Usage](#basic-usage)
- [Async Operations](#async-operations)
- [Output Handling](#output-handling)
- [Advanced Scenarios](#advanced-scenarios)
- [Error Handling](#error-handling)
- [Performance Optimization](#performance-optimization)
- [Best Practices](#best-practices)

## Overview

The Process utilities in Rinzler78.NetExtension provide enhanced capabilities for executing external processes, capturing their output, and managing process lifecycles with both synchronous and asynchronous patterns.

## Core Classes

### `ProcessHelper`
Main utility class providing extension methods for process operations:

```csharp
public static class ProcessHelper
{
    // Synchronous process execution
    public static ProcessOutputs WaitProcessOutputs(this Process process)

    // Asynchronous process execution
    public static async Task<ProcessOutputs> WaitProcessOutputsAsync(this Process process)

    // Process priority utilities
    public static ProcessPriorityClass CurrrentProcessPriorityClass()
}
```

### `ProcessOutputs`
Data structure for capturing process output:

```csharp
public class ProcessOutputs
{
    public string StandardOutput { get; }
    public string StandardError { get; }

    public ProcessOutputs(string standardOutput, string standardError)
    {
        StandardOutput = standardOutput;
        StandardError = standardError;
    }
}
```

## Basic Usage

### Simple Process Execution
```csharp
using System.Diagnostics;
using Rinzler78.NetExtension.Process;

// Create and configure process
var process = new Process();
process.StartInfo.FileName = "dotnet";
process.StartInfo.Arguments = "--version";
process.StartInfo.RedirectStandardOutput = true;
process.StartInfo.RedirectStandardError = true;
process.StartInfo.UseShellExecute = false;
process.StartInfo.CreateNoWindow = true;

// Start process
process.Start();

// Wait for completion and capture output
var outputs = process.WaitProcessOutputs();

Console.WriteLine($"Output: {outputs.StandardOutput}");
Console.WriteLine($"Error: {outputs.StandardError}");
```

### Process Priority Management
```csharp
// Get current process priority
var currentPriority = ProcessHelper.CurrrentProcessPriorityClass();
Console.WriteLine($"Current process priority: {currentPriority}");

// Set process priority
var process = Process.GetCurrentProcess();
process.PriorityClass = ProcessPriorityClass.High;
```

## Async Operations

### Asynchronous Process Execution
```csharp
public static async Task<ProcessOutputs> RunCommandAsync(string command, string arguments)
{
    using var process = new Process();
    process.StartInfo.FileName = command;
    process.StartInfo.Arguments = arguments;
    process.StartInfo.RedirectStandardOutput = true;
    process.StartInfo.RedirectStandardError = true;
    process.StartInfo.UseShellExecute = false;
    process.StartInfo.CreateNoWindow = true;

    process.Start();

    // Use async extension method
    var outputs = await process.WaitProcessOutputsAsync();

    return outputs;
}

// Usage
var outputs = await RunCommandAsync("git", "status");
Console.WriteLine($"Git status: {outputs.StandardOutput}");
```

### Parallel Process Execution
```csharp
public static async Task<ProcessOutputs[]> RunCommandsInParallelAsync(
    IEnumerable<(string command, string arguments)> commands)
{
    var tasks = commands.Select(async cmd =>
    {
        using var process = new Process();
        process.StartInfo.FileName = cmd.command;
        process.StartInfo.Arguments = cmd.arguments;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.RedirectStandardError = true;
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.CreateNoWindow = true;

        process.Start();
        return await process.WaitProcessOutputsAsync();
    });

    return await Task.WhenAll(tasks);
}

// Usage
var commands = new[]
{
    ("git", "status"),
    ("git", "log --oneline -5"),
    ("git", "branch")
};

var results = await RunCommandsInParallelAsync(commands);
foreach (var result in results)
{
    Console.WriteLine($"Output: {result.StandardOutput}");
}
```

## Output Handling

### Real-time Output Streaming
```csharp
public static async Task<ProcessOutputs> RunWithRealtimeOutputAsync(
    string command,
    string arguments,
    Action<string>? outputHandler = null,
    Action<string>? errorHandler = null)
{
    using var process = new Process();
    process.StartInfo.FileName = command;
    process.StartInfo.Arguments = arguments;
    process.StartInfo.RedirectStandardOutput = true;
    process.StartInfo.RedirectStandardError = true;
    process.StartInfo.UseShellExecute = false;
    process.StartInfo.CreateNoWindow = true;

    var outputBuilder = new StringBuilder();
    var errorBuilder = new StringBuilder();

    process.OutputDataReceived += (sender, e) =>
    {
        if (e.Data != null)
        {
            outputBuilder.AppendLine(e.Data);
            outputHandler?.Invoke(e.Data);
        }
    };

    process.ErrorDataReceived += (sender, e) =>
    {
        if (e.Data != null)
        {
            errorBuilder.AppendLine(e.Data);
            errorHandler?.Invoke(e.Data);
        }
    };

    process.Start();
    process.BeginOutputReadLine();
    process.BeginErrorReadLine();

    await process.WaitForExitAsync();

    return new ProcessOutputs(outputBuilder.ToString(), errorBuilder.ToString());
}

// Usage
var outputs = await RunWithRealtimeOutputAsync(
    "dotnet",
    "build",
    outputHandler: line => Console.WriteLine($"Build: {line}"),
    errorHandler: line => Console.WriteLine($"Error: {line}")
);
```

### Output Parsing
```csharp
public static class OutputParser
{
    public static List<string> ParseLines(this ProcessOutputs outputs)
    {
        return outputs.StandardOutput
            .Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .Select(line => line.Trim())
            .Where(line => !string.IsNullOrEmpty(line))
            .ToList();
    }

    public static Dictionary<string, string> ParseKeyValuePairs(this ProcessOutputs outputs,
        char separator = '=')
    {
        return outputs.ParseLines()
            .Where(line => line.Contains(separator))
            .Select(line => line.Split(separator, 2))
            .Where(parts => parts.Length == 2)
            .ToDictionary(parts => parts[0].Trim(), parts => parts[1].Trim());
    }

    public static bool HasErrors(this ProcessOutputs outputs)
    {
        return !string.IsNullOrEmpty(outputs.StandardError);
    }
}

// Usage
var outputs = await RunCommandAsync("dotnet", "list package");
var lines = outputs.ParseLines();
var hasErrors = outputs.HasErrors();
```

## Advanced Scenarios

### Process with Timeout
```csharp
public static async Task<ProcessOutputs> RunWithTimeoutAsync(
    string command,
    string arguments,
    TimeSpan timeout)
{
    using var process = new Process();
    process.StartInfo.FileName = command;
    process.StartInfo.Arguments = arguments;
    process.StartInfo.RedirectStandardOutput = true;
    process.StartInfo.RedirectStandardError = true;
    process.StartInfo.UseShellExecute = false;
    process.StartInfo.CreateNoWindow = true;

    process.Start();

    using var cts = new CancellationTokenSource(timeout);

    try
    {
        var outputTask = process.WaitProcessOutputsAsync();
        var completedTask = await Task.WhenAny(outputTask, Task.Delay(timeout, cts.Token));

        if (completedTask == outputTask)
        {
            return await outputTask;
        }
        else
        {
            process.Kill();
            throw new TimeoutException($"Process timed out after {timeout}");
        }
    }
    catch (OperationCanceledException)
    {
        process.Kill();
        throw new TimeoutException($"Process timed out after {timeout}");
    }
}

// Usage
try
{
    var outputs = await RunWithTimeoutAsync("slowcommand", "arguments", TimeSpan.FromSeconds(30));
    Console.WriteLine(outputs.StandardOutput);
}
catch (TimeoutException ex)
{
    Console.WriteLine($"Command timed out: {ex.Message}");
}
```

### Process with Cancellation
```csharp
public static async Task<ProcessOutputs> RunWithCancellationAsync(
    string command,
    string arguments,
    CancellationToken cancellationToken = default)
{
    using var process = new Process();
    process.StartInfo.FileName = command;
    process.StartInfo.Arguments = arguments;
    process.StartInfo.RedirectStandardOutput = true;
    process.StartInfo.RedirectStandardError = true;
    process.StartInfo.UseShellExecute = false;
    process.StartInfo.CreateNoWindow = true;

    process.Start();

    // Register cancellation callback
    using var registration = cancellationToken.Register(() =>
    {
        try
        {
            if (!process.HasExited)
            {
                process.Kill();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error killing process: {ex.Message}");
        }
    });

    try
    {
        return await process.WaitProcessOutputsAsync();
    }
    catch (Exception) when (cancellationToken.IsCancellationRequested)
    {
        throw new OperationCanceledException("Process execution was cancelled");
    }
}

// Usage
using var cts = new CancellationTokenSource();
cts.CancelAfter(TimeSpan.FromSeconds(30));

try
{
    var outputs = await RunWithCancellationAsync("longrunningcommand", "args", cts.Token);
}
catch (OperationCanceledException)
{
    Console.WriteLine("Process was cancelled");
}
```

### Process Chain Execution
```csharp
public static async Task<ProcessOutputs> RunProcessChainAsync(
    params (string command, string arguments)[] commands)
{
    var allOutputs = new List<ProcessOutputs>();

    foreach (var (command, arguments) in commands)
    {
        var outputs = await RunCommandAsync(command, arguments);
        allOutputs.Add(outputs);

        // Stop chain if any command fails
        if (outputs.HasErrors())
        {
            break;
        }
    }

    var combinedOutput = string.Join("\n", allOutputs.Select(o => o.StandardOutput));
    var combinedError = string.Join("\n", allOutputs.Select(o => o.StandardError));

    return new ProcessOutputs(combinedOutput, combinedError);
}

// Usage
var outputs = await RunProcessChainAsync(
    ("git", "pull"),
    ("dotnet", "restore"),
    ("dotnet", "build"),
    ("dotnet", "test")
);
```

## Error Handling

### Comprehensive Error Handling
```csharp
public static async Task<ProcessResult> SafeRunCommandAsync(
    string command,
    string arguments,
    TimeSpan? timeout = null)
{
    try
    {
        using var process = new Process();
        process.StartInfo.FileName = command;
        process.StartInfo.Arguments = arguments;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.RedirectStandardError = true;
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.CreateNoWindow = true;

        process.Start();

        ProcessOutputs outputs;
        if (timeout.HasValue)
        {
            outputs = await RunWithTimeoutAsync(command, arguments, timeout.Value);
        }
        else
        {
            outputs = await process.WaitProcessOutputsAsync();
        }

        return new ProcessResult
        {
            Success = process.ExitCode == 0,
            ExitCode = process.ExitCode,
            Outputs = outputs,
            Command = command,
            Arguments = arguments
        };
    }
    catch (Exception ex)
    {
        return new ProcessResult
        {
            Success = false,
            Exception = ex,
            Command = command,
            Arguments = arguments
        };
    }
}

public class ProcessResult
{
    public bool Success { get; set; }
    public int ExitCode { get; set; }
    public ProcessOutputs? Outputs { get; set; }
    public Exception? Exception { get; set; }
    public string Command { get; set; } = string.Empty;
    public string Arguments { get; set; } = string.Empty;
}

// Usage
var result = await SafeRunCommandAsync("git", "status");
if (result.Success)
{
    Console.WriteLine($"Command succeeded: {result.Outputs?.StandardOutput}");
}
else
{
    Console.WriteLine($"Command failed: {result.Exception?.Message}");
}
```

## Performance Optimization

### Process Pooling
```csharp
public class ProcessPool : IDisposable
{
    private readonly ConcurrentQueue<Process> _processes = new();
    private readonly string _command;
    private readonly int _maxPoolSize;
    private int _currentPoolSize;

    public ProcessPool(string command, int maxPoolSize = 10)
    {
        _command = command;
        _maxPoolSize = maxPoolSize;
    }

    public async Task<ProcessOutputs> ExecuteAsync(string arguments)
    {
        var process = GetProcess();

        try
        {
            process.StartInfo.Arguments = arguments;
            process.Start();

            return await process.WaitProcessOutputsAsync();
        }
        finally
        {
            ReturnProcess(process);
        }
    }

    private Process GetProcess()
    {
        if (_processes.TryDequeue(out var process))
        {
            return process;
        }

        return CreateProcess();
    }

    private Process CreateProcess()
    {
        var process = new Process();
        process.StartInfo.FileName = _command;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.RedirectStandardError = true;
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.CreateNoWindow = true;

        Interlocked.Increment(ref _currentPoolSize);
        return process;
    }

    private void ReturnProcess(Process process)
    {
        if (_currentPoolSize <= _maxPoolSize && !process.HasExited)
        {
            _processes.Enqueue(process);
        }
        else
        {
            process.Dispose();
            Interlocked.Decrement(ref _currentPoolSize);
        }
    }

    public void Dispose()
    {
        while (_processes.TryDequeue(out var process))
        {
            process.Dispose();
        }
    }
}

// Usage
using var pool = new ProcessPool("git");
var outputs = await pool.ExecuteAsync("status");
```

## Best Practices

### 1. Resource Management
```csharp
public static async Task<ProcessOutputs> SafeExecuteAsync(string command, string arguments)
{
    using var process = new Process();

    try
    {
        process.StartInfo.FileName = command;
        process.StartInfo.Arguments = arguments;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.RedirectStandardError = true;
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.CreateNoWindow = true;

        process.Start();
        return await process.WaitProcessOutputsAsync();
    }
    finally
    {
        // Ensure process is cleaned up
        if (!process.HasExited)
        {
            process.Kill();
        }
    }
}
```

### 2. Security Considerations
```csharp
public static class SecureProcessHelper
{
    public static async Task<ProcessOutputs> RunSecureCommandAsync(
        string command,
        string arguments,
        string? workingDirectory = null,
        Dictionary<string, string>? environmentVariables = null)
    {
        // Validate command path
        if (!File.Exists(command) && !IsInPath(command))
        {
            throw new FileNotFoundException($"Command not found: {command}");
        }

        using var process = new Process();
        process.StartInfo.FileName = command;
        process.StartInfo.Arguments = arguments;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.RedirectStandardError = true;
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.CreateNoWindow = true;

        if (workingDirectory != null)
        {
            process.StartInfo.WorkingDirectory = workingDirectory;
        }

        if (environmentVariables != null)
        {
            foreach (var kvp in environmentVariables)
            {
                process.StartInfo.EnvironmentVariables[kvp.Key] = kvp.Value;
            }
        }

        process.Start();
        return await process.WaitProcessOutputsAsync();
    }

    private static bool IsInPath(string command)
    {
        var pathEnv = Environment.GetEnvironmentVariable("PATH");
        if (string.IsNullOrEmpty(pathEnv))
            return false;

        var paths = pathEnv.Split(Path.PathSeparator);
        return paths.Any(path => File.Exists(Path.Combine(path, command)));
    }
}
```

### 3. Logging and Monitoring
```csharp
public static async Task<ProcessOutputs> RunWithLoggingAsync(
    string command,
    string arguments,
    ILogger? logger = null)
{
    logger?.LogInformation("Starting process: {Command} {Arguments}", command, arguments);

    var stopwatch = Stopwatch.StartNew();

    try
    {
        var outputs = await RunCommandAsync(command, arguments);

        stopwatch.Stop();
        logger?.LogInformation("Process completed in {ElapsedMs}ms", stopwatch.ElapsedMilliseconds);

        return outputs;
    }
    catch (Exception ex)
    {
        stopwatch.Stop();
        logger?.LogError(ex, "Process failed after {ElapsedMs}ms", stopwatch.ElapsedMilliseconds);
        throw;
    }
}
```

### 4. Configuration Management
```csharp
public class ProcessConfiguration
{
    public string Command { get; set; } = string.Empty;
    public string Arguments { get; set; } = string.Empty;
    public string? WorkingDirectory { get; set; }
    public Dictionary<string, string> EnvironmentVariables { get; set; } = new();
    public TimeSpan Timeout { get; set; } = TimeSpan.FromMinutes(5);
    public int MaxRetries { get; set; } = 3;
    public bool CaptureOutput { get; set; } = true;
}

public static async Task<ProcessOutputs> RunConfiguredAsync(ProcessConfiguration config)
{
    using var process = new Process();
    process.StartInfo.FileName = config.Command;
    process.StartInfo.Arguments = config.Arguments;
    process.StartInfo.RedirectStandardOutput = true;
    process.StartInfo.RedirectStandardError = true;
    process.StartInfo.UseShellExecute = false;
    process.StartInfo.CreateNoWindow = true;

    process.Start();
    return await process.WaitProcessOutputsAsync();
}
```

---

[← Back to Main Documentation](../README.md)
