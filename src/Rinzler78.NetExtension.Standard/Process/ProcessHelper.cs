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
        var stdOut = await process.StandardOutput.ReadToEndAsync();
        var stdErr = await process.StandardError.ReadToEndAsync();

        await process.WaitForExitAsync();

        return new ProcessOutputs(stdOut, stdErr);
    }
}