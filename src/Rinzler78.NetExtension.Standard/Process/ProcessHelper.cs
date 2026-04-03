using System.Diagnostics;
using System.Threading.Tasks;

namespace Rinzler78.NetExtension.Process;

public static class ProcessHelper
{
    public static ProcessOutputs WaitProcessOutputs(this System.Diagnostics.Process process)
    {
        var stdOut = process.StandardOutput.ReadToEnd();
        var stdErr = process.StandardError.ReadToEnd();

        process.WaitForExit();

        return new ProcessOutputs(stdOut, stdErr);
    }

    public static async Task<ProcessOutputs> WaitProcessOutputsAsync(this System.Diagnostics.Process process)
    {
        var stdOut = await process.StandardOutput.ReadToEndAsync().ConfigureAwait(false);
        var stdErr = await process.StandardError.ReadToEndAsync().ConfigureAwait(false);

        await process.WaitForExitAsync().ConfigureAwait(false);

        return new ProcessOutputs(stdOut, stdErr);
    }

    public static ProcessPriorityClass CurrrentProcessPriorityClass()
        => System.Diagnostics.Process.GetCurrentProcess().PriorityClass;
}
