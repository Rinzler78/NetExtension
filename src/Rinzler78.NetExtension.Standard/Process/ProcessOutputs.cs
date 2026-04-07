namespace Rinzler78.NetExtension.Process;

/// <summary>
/// Holds the standard output and standard error streams captured from a process execution.
/// </summary>
public sealed class ProcessOutputs
{
    /// <summary>
    /// Initializes a new instance of <see cref="ProcessOutputs"/> with the captured output streams.
    /// </summary>
    /// <param name="stdOut">The standard output content.</param>
    /// <param name="stdErr">The standard error content.</param>
    public ProcessOutputs(string stdOut, string stdErr)
    {
        StdOut = stdOut;
        StdErr = stdErr;
    }

    /// <summary>
    /// Gets the captured standard output content.
    /// </summary>
    public string StdOut { get; }

    /// <summary>
    /// Gets the captured standard error content.
    /// </summary>
    public string StdErr { get; }

    public override string ToString()
    {
        return $"- StdOut : {StdOut}\n- StdErr : {StdErr}";
    }
}
