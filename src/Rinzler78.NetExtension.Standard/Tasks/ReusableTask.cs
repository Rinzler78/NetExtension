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

    //readonly Func<(Task Task, CancellationTokenSource CancellationTokenSource)> _taskContext;

    //public ReusableTask(Func<(Task Task, CancellationTokenSource CancellationTokenSource)> taskContext)
    //{
    //    _taskContext = taskContext;
    //}

    //(Task Task, CancellationTokenSource CancellationTokenSource) _currentRoutine;
    //Task CurrentTask => _currentRoutine.Task;
    //CancellationTokenSource CurrentCancellationTokenSource => _currentRoutine.CancellationTokenSource;

    //public Task Run()
    //{
    //    if (CurrentTask?.IsCompleted ?? true)
    //    {
    //        _currentRoutine = _taskContext.Invoke();
    //        CurrentTask.ContinueWith(t =>
    //        {
    //            Console.WriteLine($"Task {t.Status}");
    //        });
    //    }

    //    return CurrentTask;
    //}

    //public bool Cancel()
    //{
    //    lock (_taskContext)
    //    {
    //        if (CurrentCancellationTokenSource is not null)
    //        {
    //            CurrentCancellationTokenSource.Cancel();
    //            return true;
    //        }
    //    }
    //    return false;
    //}
}