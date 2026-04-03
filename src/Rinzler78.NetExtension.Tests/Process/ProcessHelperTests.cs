using System.Diagnostics;
using System.Runtime.InteropServices;
using Rinzler78.NetExtension.Process;

namespace Rinzler78.NetExtension.Tests.Process;

// All tests in this class spawn real OS processes and are therefore integration tests.
// True unit tests (no process spawning) live in ProcessHelperUnitTests.cs.
[Trait("Category", "Integration")]
public class ProcessHelperTests
{
    [Fact]
    public void WaitProcessOutputs_ShouldReturnStandardOutputAndError()
    {
        var process = new System.Diagnostics.Process();
        process.StartInfo.FileName = "echo";
        process.StartInfo.Arguments = "Hello World";
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.RedirectStandardError = true;
        process.Start();

        var result = process.WaitProcessOutputs();

        result.StdOut.Should().Contain("Hello World");
        result.StdErr.Should().BeEmpty();
    }

    [Fact]
    public async Task WaitProcessOutputsAsync_ShouldReturnStandardOutputAndError()
    {
        var process = new System.Diagnostics.Process();
        process.StartInfo.FileName = "echo";
        process.StartInfo.Arguments = "Hello World";
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.RedirectStandardError = true;
        process.Start();

        var result = await process.WaitProcessOutputsAsync();

        result.StdOut.Should().Contain("Hello World");
        result.StdErr.Should().BeEmpty();
    }

    [Fact]
    public void WaitProcessOutputs_WithErrorOutput_ShouldCaptureError()
    {
        var process = new System.Diagnostics.Process();
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            process.StartInfo.FileName = "cmd.exe";
            process.StartInfo.Arguments = "/c \"echo 'Test error' >&2\"";
        }
        else
        {
            process.StartInfo.FileName = "sh";
            process.StartInfo.Arguments = "-c \"echo 'Test error' >&2\"";
        }
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.RedirectStandardError = true;
        process.Start();

        var result = process.WaitProcessOutputs();

        result.StdErr.Should().Contain("Test error");
    }

    [Fact]
    public async Task WaitProcessOutputsAsync_WithErrorOutput_ShouldCaptureError()
    {
        var process = new System.Diagnostics.Process();
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            process.StartInfo.FileName = "cmd.exe";
            process.StartInfo.Arguments = "/c \"echo 'Test error' >&2\"";
        }
        else
        {
            process.StartInfo.FileName = "sh";
            process.StartInfo.Arguments = "-c \"echo 'Test error' >&2\"";
        }
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.RedirectStandardError = true;
        process.Start();

        var result = await process.WaitProcessOutputsAsync();

        result.StdErr.Should().Contain("Test error");
    }

    [Fact]
    public void WaitProcessOutputs_WithLongRunningProcess_ShouldWaitForCompletion()
    {
        var process = new System.Diagnostics.Process();

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            process.StartInfo.FileName = "ping";
            process.StartInfo.Arguments = "127.0.0.1 -n 3";
        }
        else
        {
            process.StartInfo.FileName = "sleep";
            process.StartInfo.Arguments = "2";
        }

        process.StartInfo.UseShellExecute = false;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.RedirectStandardError = true;
        process.Start();

        var stopwatch = Stopwatch.StartNew();
        process.WaitProcessOutputs();
        stopwatch.Stop();

        stopwatch.ElapsedMilliseconds.Should().BeGreaterThan(1000);
    }

    [Fact]
    public async Task WaitProcessOutputsAsync_WithLongRunningProcess_ShouldWaitForCompletion()
    {
        var process = new System.Diagnostics.Process();

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            process.StartInfo.FileName = "ping";
            process.StartInfo.Arguments = "127.0.0.1 -n 3";
        }
        else
        {
            process.StartInfo.FileName = "sleep";
            process.StartInfo.Arguments = "2";
        }

        process.StartInfo.UseShellExecute = false;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.RedirectStandardError = true;
        process.Start();

        var stopwatch = Stopwatch.StartNew();
        await process.WaitProcessOutputsAsync();
        stopwatch.Stop();

        stopwatch.ElapsedMilliseconds.Should().BeGreaterThan(1000);
    }

    [Fact]
    public void WaitProcessOutputs_WithEmptyOutput_ShouldReturnEmptyStrings()
    {
        var process = new System.Diagnostics.Process();
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            process.StartInfo.FileName = "cmd.exe";
            process.StartInfo.Arguments = "/c rem";
        }
        else
        {
            process.StartInfo.FileName = "true";
            process.StartInfo.Arguments = "";
        }
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.RedirectStandardError = true;
        process.Start();

        var result = process.WaitProcessOutputs();

        result.StdOut.Should().BeEmpty();
        result.StdErr.Should().BeEmpty();
    }

    [Fact]
    public async Task WaitProcessOutputsAsync_WithEmptyOutput_ShouldReturnEmptyStrings()
    {
        var process = new System.Diagnostics.Process();
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            process.StartInfo.FileName = "cmd.exe";
            process.StartInfo.Arguments = "/c rem";
        }
        else
        {
            process.StartInfo.FileName = "true";
            process.StartInfo.Arguments = "";
        }
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.RedirectStandardError = true;
        process.Start();

        var result = await process.WaitProcessOutputsAsync();

        result.StdOut.Should().BeEmpty();
        result.StdErr.Should().BeEmpty();
    }
}
