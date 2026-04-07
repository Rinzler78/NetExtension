using System;
using System.Threading;
using System.Threading.Tasks;

namespace Rinzler78.NetExtension.Tasks;

/// <summary>
/// Wraps a synchronous <see cref="Action"/> and allows single-flight asynchronous invocation
/// with cancellation support. Implements <see cref="IDisposable"/> to release
/// <see cref="CancellationTokenSource"/> resources.
/// </summary>
public sealed class ReusableTask : IDisposable
{
    private CancellationTokenSource _cancellationTokenSource = new();
    private Task _invokeTask = Task.CompletedTask;
    private readonly object _lock = new();
    private bool _taskEverStarted;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of <see cref="ReusableTask"/> with the specified action.
    /// </summary>
    /// <param name="action">The action to execute. Must not be null.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="action"/> is null.</exception>
    public ReusableTask(Action action)
    {
        Action = action ?? throw new ArgumentNullException(nameof(action));
    }

    private Action Action { get; }

    /// <summary>
    /// Invokes the wrapped action asynchronously. If a previous invocation is still running,
    /// returns the same in-progress <see cref="Task"/> without starting a new one.
    /// Disposes the previous <see cref="CancellationTokenSource"/> before creating a new one.
    /// </summary>
    /// <returns>The current or newly started <see cref="Task"/>.</returns>
    /// <exception cref="ObjectDisposedException">Thrown when called after <see cref="Dispose"/>.</exception>
    public Task Invoke()
    {
        lock (_lock)
        {
            ThrowIfDisposed();

            if (!_invokeTask.IsCompleted)
                return _invokeTask;

            // Rotate CancellationTokenSource: dispose old before creating new
            var oldCts = _cancellationTokenSource;
            _cancellationTokenSource = new CancellationTokenSource();
            oldCts.Dispose();

            _taskEverStarted = true;
            var token = _cancellationTokenSource.Token;
            _invokeTask = Task.Run(() =>
            {
                if (!token.IsCancellationRequested)
                    Action();
            }, token);

            return _invokeTask;
        }
    }

    /// <summary>
    /// Executes the wrapped action synchronously on the calling thread,
    /// provided cancellation has not been requested.
    /// </summary>
    /// <exception cref="ObjectDisposedException">Thrown when called after <see cref="Dispose"/>.</exception>
    public void InvokeSync()
    {
        lock (_lock)
        {
            ThrowIfDisposed();

            if (_cancellationTokenSource.Token.IsCancellationRequested)
                return;

            Action();
        }
    }

    /// <summary>
    /// Requests cancellation of the current in-progress action.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> if a task had been started and cancellation was newly requested;
    /// <see langword="false"/> if no task was ever started, or if the token was already cancelled.
    /// </returns>
    /// <exception cref="ObjectDisposedException">Thrown when called after <see cref="Dispose"/>.</exception>
    public bool Cancel()
    {
        lock (_lock)
        {
            ThrowIfDisposed();

            if (!_taskEverStarted)
                return false;

            if (_cancellationTokenSource.IsCancellationRequested)
                return false;

            _cancellationTokenSource.Cancel();
            return true;
        }
    }

    /// <summary>
    /// Releases the <see cref="CancellationTokenSource"/> used by this instance.
    /// Subsequent calls to <see cref="Invoke"/>, <see cref="InvokeSync"/>, or
    /// <see cref="Cancel"/> will throw <see cref="ObjectDisposedException"/>.
    /// </summary>
    public void Dispose()
    {
        lock (_lock)
        {
            if (_disposed)
                return;

            _cancellationTokenSource.Cancel();
            _cancellationTokenSource.Dispose();
            _disposed = true;
        }
    }

    private void ThrowIfDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(ReusableTask));
    }
}
