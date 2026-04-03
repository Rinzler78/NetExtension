using Rinzler78.NetExtension.Tasks;

namespace Rinzler78.NetExtension.Tests.Tasks;

[Trait("Category", "Unit")]
public class ReusableTaskAdditionalTests
{
    [Fact]
    public void Constructor_WithNullAction_ShouldThrowArgumentNullException()
    {
        Action act = () => _ = new ReusableTask(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void InvokeSync_WhenCancelled_ShouldNotExecuteAction()
    {
        var calls = 0;
        var task = new ReusableTask(() => Interlocked.Increment(ref calls));
        task.Cancel();

        task.InvokeSync();

        calls.Should().Be(0);
    }

    [Fact]
    public void InvokeSync_WhenActive_ShouldExecuteAction()
    {
        var calls = 0;
        var task = new ReusableTask(() => Interlocked.Increment(ref calls));

        task.InvokeSync();

        calls.Should().Be(1);
    }

    [Fact]
    public void Cancel_ShouldReturnTrue()
    {
        var task = new ReusableTask(() => { });

        task.Cancel().Should().BeTrue();
    }
}
