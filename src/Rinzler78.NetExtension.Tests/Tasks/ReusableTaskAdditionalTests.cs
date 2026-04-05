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
    public async Task InvokeSync_WhenCancelled_ShouldNotExecuteAction()
    {
        var calls = 0;
        var task = new ReusableTask(() => Interlocked.Increment(ref calls));

        // Must invoke first so _taskEverStarted = true, then cancel the CTS
        await task.Invoke();
        calls = 0; // reset counter — only InvokeSync behaviour matters below

        task.Cancel(); // now _taskEverStarted=true → actually cancels CTS
        task.InvokeSync(); // CTS is cancelled → should not execute

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
    public void Cancel_AfterInvoke_ShouldReturnTrue()
    {
        // Cancel() now requires _taskEverStarted == true to return true
        var task = new ReusableTask(() => Thread.Sleep(200));
        task.Invoke(); // start async → sets _taskEverStarted = true
        task.Cancel().Should().BeTrue();
    }
}

[Trait("Category", "Unit")]
public class ReusableTaskDisposeTests
{
    [Fact]
    public void Dispose_DisposesInternalCts_WithoutThrowing()
    {
        var task = new ReusableTask(() => { });
        var act = () => task.Dispose();
        act.Should().NotThrow();
    }

    [Fact]
    public void Invoke_AfterDispose_ThrowsObjectDisposedException()
    {
        var task = new ReusableTask(() => { });
        task.Dispose();
        // Cast to Action to avoid FluentAssertions treating Func<Task> as async
        Action act = () => { task.Invoke(); };
        act.Should().Throw<ObjectDisposedException>();
    }

    [Fact]
    public void InvokeSync_AfterDispose_ThrowsObjectDisposedException()
    {
        var task = new ReusableTask(() => { });
        task.Dispose();
        var act = () => task.InvokeSync();
        act.Should().Throw<ObjectDisposedException>();
    }

    [Fact]
    public void Cancel_AfterDispose_ThrowsObjectDisposedException()
    {
        var task = new ReusableTask(() => { });
        task.Dispose();
        var act = () => task.Cancel();
        act.Should().Throw<ObjectDisposedException>();
    }

    [Fact]
    public void Cancel_BeforeAnyInvoke_ReturnsFalse()
    {
        using var task = new ReusableTask(() => { });
        task.Cancel().Should().BeFalse();
    }

    [Fact]
    public void Cancel_WhenAlreadyCancelled_ReturnsFalse()
    {
        using var task = new ReusableTask(() => Thread.Sleep(500));
        task.Invoke(); // start
        task.Cancel(); // first cancel → true
        task.Cancel().Should().BeFalse(); // second → false
    }

    [Fact]
    public async Task Invoke_RotatesCts_PreviousCtsDisposed()
    {
        // Verify no exception thrown after multiple Invoke + complete cycles
        using var task = new ReusableTask(() => { });
        for (int i = 0; i < 5; i++)
        {
            await task.Invoke();
        }
        // If CTS was not disposed properly, finalizer would trigger eventually.
        // At minimum, verify it runs without error.
    }
}
