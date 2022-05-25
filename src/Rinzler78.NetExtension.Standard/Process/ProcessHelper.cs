using System;
using System.Threading.Tasks;

namespace Rinzler78.NetExtension.Process
{

    public static class ProcessHelper
    {
        public static ProcessOutputs WaitProcessOutputs(this System.Diagnostics.Process process)
        {
            process.WaitForExit();

            return new(process.StandardOutput.ReadToEnd(), process.StandardError.ReadToEnd());
        }

        public static async Task<ProcessOutputs> WaitProcessOutputsAsync(this System.Diagnostics.Process process)
        {
            await process.WaitForExitAsync();

            return new(process.StandardOutput.ReadToEnd(), process.StandardError.ReadToEnd());
        }
    }
}

