using Rinzler78.NetExtension.Process;

namespace Rinzler78.NetExtension.Tests.Process;

[Trait("Category", "Unit")]
public class ProcessOutputsTests
{
    [Fact]
    public void ToString_ShouldExposeStdOutAndStdErr()
    {
        var outputs = new ProcessOutputs("ok", "warn");

        outputs.StdOut.Should().Be("ok");
        outputs.StdErr.Should().Be("warn");
        outputs.ToString().Should().Contain("StdOut").And.Contain("StdErr");
    }
}
