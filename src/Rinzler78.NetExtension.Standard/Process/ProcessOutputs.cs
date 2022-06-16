namespace Rinzler78.NetExtension.Process;

public class ProcessOutputs
{
    public ProcessOutputs(string stdOut, string stdErr)
    {
        StdOut = stdOut;
        StdErr = stdErr;
    }

    public string StdOut { get; }
    public string StdErr { get; }
}