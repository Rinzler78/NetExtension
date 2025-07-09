using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Rinzler78.NetExtension.Tasks;

public static class TaskHelper
{
    public static bool IsCancellationRequested(this CancellationTokenSource cancellationTokenSource)
    {
        return cancellationTokenSource?.IsCancellationRequested ?? true;
    }

    public static bool IsRunning(this Task task)
    {
        return !(task?.IsCompleted ?? true);
    }

    public static Task WhenAll(this IEnumerable<Task> tasks)
    {
        return Task.WhenAll(tasks);
    }
}
