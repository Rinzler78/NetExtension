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

    [Fact]
    public void Constructor_WithEmptyStrings_ShouldStoreEmptyValues()
    {
        var outputs = new ProcessOutputs("", "");

        outputs.StdOut.Should().BeEmpty();
        outputs.StdErr.Should().BeEmpty();
    }

    [Fact]
    public void Constructor_WithNullValues_ShouldStoreNulls()
    {
        var outputs = new ProcessOutputs(null!, null!);

        outputs.StdOut.Should().BeNull();
        outputs.StdErr.Should().BeNull();
    }

    [Fact]
    public void Properties_ShouldBeReadOnly()
    {
        var outputs = new ProcessOutputs("stdout content", "stderr content");

        outputs.StdOut.Should().Be("stdout content");
        outputs.StdErr.Should().Be("stderr content");
    }
}
