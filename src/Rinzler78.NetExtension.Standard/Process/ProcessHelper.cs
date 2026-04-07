using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Rinzler78.NetExtension.Process;

/// <summary>
/// Provides extension methods for <see cref="System.Diagnostics.Process"/> to capture
/// output streams and query the current process priority.
/// </summary>
public static class ProcessHelper
{
    /// <summary>
    /// Reads both stdout and stderr concurrently, then waits for the process to exit.
    /// </summary>
    /// <param name="process">The process whose streams to capture.</param>
    /// <returns>A <see cref="ProcessOutputs"/> containing both streams.</returns>
    /// <remarks>
    /// Both streams are read concurrently via <c>Task.WhenAll</c> to prevent the OS pipe
    /// buffer deadlock that occurs when one stream fills while the other is being read
    /// synchronously.
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="process"/> is null.</exception>
    public static async Task<ProcessOutputs> WaitProcessOutputsAsync(this System.Diagnostics.Process process)
    {
        ArgumentNullException.ThrowIfNull(process);

        var stdOutTask = process.StandardOutput.ReadToEndAsync();
        var stdErrTask = process.StandardError.ReadToEndAsync();

        await Task.WhenAll(stdOutTask, stdErrTask).ConfigureAwait(false);
        await process.WaitForExitAsync().ConfigureAwait(false);

        return new ProcessOutputs(await stdOutTask, await stdErrTask);
    }

    /// <summary>
    /// Synchronously reads both stdout and stderr and waits for the process to exit.
    /// </summary>
    /// <param name="process">The process whose streams to capture.</param>
    /// <returns>A <see cref="ProcessOutputs"/> containing both streams.</returns>
    /// <remarks>
    /// This overload blocks the calling thread. Prefer <see cref="WaitProcessOutputsAsync"/>
    /// in async contexts to avoid potential deadlocks on synchronization-context-bound threads.
    /// Internally delegates to <see cref="WaitProcessOutputsAsync"/> with parallel stream reads.
    /// </remarks>
    [Obsolete("Prefer WaitProcessOutputsAsync to avoid pipe deadlock on large output and SynchronizationContext deadlocks.")]
    public static ProcessOutputs WaitProcessOutputs(this System.Diagnostics.Process process)
        => process.WaitProcessOutputsAsync().GetAwaiter().GetResult();

    /// <summary>
    /// Returns the <see cref="ProcessPriorityClass"/> of the currently executing process.
    /// </summary>
    public static ProcessPriorityClass CurrentProcessPriorityClass()
    {
        using var process = System.Diagnostics.Process.GetCurrentProcess();
        return process.PriorityClass;
    }
}
