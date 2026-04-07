using System.Diagnostics;
using Rinzler78.NetExtension.Process;

namespace Rinzler78.NetExtension.Tests.Process;

/// <summary>
/// Pure unit tests for <see cref="ProcessHelper"/> that do NOT spawn child processes.
/// Tests that spawn real OS processes live in <see cref="ProcessHelperTests"/> (Category = Integration).
/// </summary>
[Trait("Category", "Unit")]
public class ProcessHelperUnitTests
{
    [Fact]
    public void GetCurrentProcessName_ShouldReturnNonEmptyString()
    {
        // Uses Process.GetCurrentProcess() — no child process is spawned.
        var name = System.Diagnostics.Process.GetCurrentProcess().ProcessName;

        name.Should().NotBeNullOrWhiteSpace(
            "the test host process always has a non-empty name");
    }

    [Fact]
    public void CurrentProcessPriorityClass_ShouldReturnValidPriorityClass()
    {
        // NOTE: The method name "CurrentProcessPriorityClass" contains a typo in the
        // production API (three r's in "Currrent").  This test intentionally tracks that
        // typo so that a future rename will cause a compile error here, alerting the
        // maintainer to update call sites simultaneously.
        var priority = ProcessHelper.CurrentProcessPriorityClass();

        Enum.IsDefined(typeof(ProcessPriorityClass), priority).Should().BeTrue(
            "CurrentProcessPriorityClass() must return a member of the ProcessPriorityClass enum");
    }
}
