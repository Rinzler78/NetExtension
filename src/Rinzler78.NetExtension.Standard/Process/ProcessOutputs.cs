namespace Rinzler78.NetExtension.Process;

public sealed class ProcessOutputs
{
    public ProcessOutputs(string stdOut, string stdErr)
    {
        StdOut = stdOut;
        StdErr = stdErr;
    }

    public string StdOut { get; }
    public string StdErr { get; }

    public override string ToString()
    {
        return $"- StdOut : {StdOut}\n- StdErr : {StdErr}";
    }
}