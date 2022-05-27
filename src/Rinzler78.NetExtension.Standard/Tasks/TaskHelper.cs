using System.Threading.Tasks;

namespace Rinzler78.NetExtension.Tasks;

public static class TaskHelper
{
    public static Task WhenAll(this Task[] tasks)
    {
        return Task.WhenAll(tasks);
    }
}