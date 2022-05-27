using System.Threading.Tasks;

namespace Rinzler78.NetExtension.Process;

public static class ProcessHelper
{
    public static ProcessOutputs WaitProcessOutputs(this System.Diagnostics.Process process)
    {
        process.WaitForExit();

        return new ProcessOutputs(process.StandardOutput.ReadToEnd(), process.StandardError.ReadToEnd());
    }

    public static async Task<ProcessOutputs> WaitProcessOutputsAsync(this System.Diagnostics.Process process)
    {
        await process.WaitForExitAsync().ConfigureAwait(false);

        return new ProcessOutputs(process.StandardOutput.ReadToEnd(), process.StandardError.ReadToEnd());
    }
}