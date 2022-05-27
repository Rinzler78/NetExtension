using System;
using System.Threading;
using System.Threading.Tasks;

namespace Rinzler78.NetExtension.Tasks;

public class ReusableTask
{
    private CancellationTokenSource _cancellationTokenSource;

    private Task _invokeTask;

    public ReusableTask(Action action)
    {
        Action = action;
    }

    private Action Action { get; }

    public Task Invoke()
    {
        lock (this)
        {
            if (_invokeTask?.IsCompleted ?? true) _invokeTask = Task.Run(() => Action());
            return _invokeTask;
        }
    }

    public bool Cancel()
    {
        lock (this)
        {
            if (_cancellationTokenSource != null)
            {
                _cancellationTokenSource.Cancel();
                _cancellationTokenSource = null;

                return true;
            }

            return false;
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
    //        if (CurrentCancellationTokenSource != null)
    //        {
    //            CurrentCancellationTokenSource.Cancel();
    //            return true;
    //        }
    //    }
    //    return false;
    //}
}