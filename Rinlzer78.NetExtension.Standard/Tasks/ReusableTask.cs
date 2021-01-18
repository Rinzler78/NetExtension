using System;
using System.Threading;
using System.Threading.Tasks;

namespace Rinlzer78.NetExtension.Tasks
{
    public class ReusableTask
    {
        readonly Func<(Task Task, CancellationTokenSource CancellationTokenSource)> _taskContext;

        public ReusableTask(Func<(Task Task, CancellationTokenSource CancellationTokenSource)> taskContext)
        {
            _taskContext = taskContext;
        }

        (Task Task, CancellationTokenSource CancellationTokenSource) _currentRoutine;
        Task CurrentTask => _currentRoutine.Task;
        CancellationTokenSource CurrentCancellationTokenSource => _currentRoutine.CancellationTokenSource;

        public Task Run()
        {
            if (CurrentTask?.IsCompleted ?? true)
            {
                _currentRoutine = _taskContext.Invoke();
                CurrentTask.ContinueWith(t =>
                {
                    Console.WriteLine($"Task {t.Status}");
                });
            }

            return CurrentTask;
        }

        public bool Cancel()
        {
            lock (_taskContext)
            {
                if (CurrentCancellationTokenSource != null)
                {
                    CurrentCancellationTokenSource.Cancel();
                    return true;
                }
            }
            return false;
        }
    }
}
