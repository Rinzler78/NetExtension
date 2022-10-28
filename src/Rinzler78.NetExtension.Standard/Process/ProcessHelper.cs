using System.Threading.Tasks;

namespace Rinzler78.NetExtension.Process;

public static class ProcessHelper
{
    public static ProcessOutputs WaitProcessOutputs(this System.Diagnostics.Process process)
    {
        var stds = Task.WhenAll(process.StandardOutput.ReadToEndAsync(), process.StandardError.ReadToEndAsync()).Result;

        var stdOut = stds[0];
        var stdErr = stds[1];

        //process.WaitForExit();

        return new ProcessOutputs(stdOut, stdErr);
    }

    //public static async Task<ProcessOutputs> WaitProcessOutputsAsync(this System.Diagnostics.Process process)
    //{
    //    var stdOut = process.StandardOutput.ReadToEnd();
    //    var stdErr = process.StandardError.ReadToEnd();

    //    await process.WaitForExitAsync().ConfigureAwait(false);

    //    return new ProcessOutputs(stdOut, stdErr);
    //}
}