using System;
using System.Threading;
using System.Threading.Tasks;

namespace Rinzler78.NetExtension.Tasks;

public sealed class ReusableTask
{
    private CancellationTokenSource _cancellationTokenSource = new();
    private Task _invokeTask = Task.CompletedTask;
    private readonly object _lock = new();

    public ReusableTask(Action action)
    {
        Action = action ?? throw new ArgumentNullException(nameof(action));
    }

    private Action Action { get; }

    public Task Invoke()
    {
        lock (_lock)
        {
            if (!_invokeTask.IsCompleted)
                return _invokeTask;

            _cancellationTokenSource = new CancellationTokenSource();
            _invokeTask = Task.Run(() =>
            {
                if (!_cancellationTokenSource.Token.IsCancellationRequested)
                {
                    Action();
                }
            }, _cancellationTokenSource.Token);

            return _invokeTask;
        }
    }

    public void InvokeSync()
    {
        lock (_lock)
        {
            if (_cancellationTokenSource.Token.IsCancellationRequested)
                return;

            Action();
        }
    }

    public bool Cancel()
    {
        lock (_lock)
        {
            _cancellationTokenSource.Cancel();
            return true;
        }
    }

}
