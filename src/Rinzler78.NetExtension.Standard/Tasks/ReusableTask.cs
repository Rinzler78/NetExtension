using System;
using System.Threading;
using System.Threading.Tasks;

namespace Rinzler78.NetExtension.Tasks;

public sealed class ReusableTask
{
    private CancellationTokenSource _cancellationTokenSource = new(0);

    private Task _invokeTask = Task.CompletedTask;

    public ReusableTask(Action action)
    {
        Action = action;
    }

    private Action Action { get; }

    public Task Invoke()
    {
        lock (_invokeTask)
        {
            if (!_invokeTask.IsCompleted) 
                return _invokeTask;
            
            _cancellationTokenSource = new CancellationTokenSource();
            _invokeTask = Task.Run(() => Action(), _cancellationTokenSource.Token);

            return _invokeTask;
        }
    }

    public bool Cancel()
    {
        lock (_cancellationTokenSource)
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