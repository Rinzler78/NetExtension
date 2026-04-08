using System.Threading;
using Rinzler78.NetExtension.Measure;

namespace Rinzler78.NetExtension.Tests.Measure;

[Trait("Category", "Unit")]
public class PerformanceHelperTests
{
    [Fact]
    public void MeasureDuration_ActionWithOut_ShouldCaptureElapsedTime()
    {
        Action action = () => Thread.Sleep(10);

        action.MeasureDuration(out var duration);

        duration.Should().BeGreaterThan(TimeSpan.Zero);
    }

    [Fact]
    public void MeasureDuration_ActionCallback_ShouldInvokeCallback()
    {
        // A no-op delegate can complete in 0 ticks on fast hardware, so
        // BeGreaterThanOrEqualTo(Zero) avoids a flaky failure.
        TimeSpan captured = TimeSpan.MinValue;
        Action action = () => { };

        action.MeasureDuration(duration => captured = duration);

        captured.Should().BeGreaterThanOrEqualTo(TimeSpan.Zero,
            "the callback must be called and the captured duration must be non-negative");
    }

    [Fact]
    public void MeasureDuration_WithNonTrivialAction_ShouldMeasureActualDuration()
    {
        // Uses a deliberate 10 ms sleep to verify the elapsed time is actually captured.
        Action action = () => Thread.Sleep(10);
        TimeSpan captured = TimeSpan.Zero;

        action.MeasureDuration(duration => captured = duration);

        captured.Should().BeGreaterThanOrEqualTo(TimeSpan.FromMilliseconds(5),
            "a 10 ms sleep must produce a measurable non-trivial duration");
    }

    [Fact]
    public void MeasureDuration_WhenActionThrows_CallbackIsNotInvoked()
    {
        // Documents current contract: because MeasureDuration does not use try/finally,
        // if the measured delegate throws then the duration callback is NOT invoked.
        // If a future implementation adds try/finally behaviour, this test will fail
        // and should be updated together with the production code.
        var callbackInvoked = false;
        Action throwingAction = () => throw new InvalidOperationException("boom");

        var act = () => throwingAction.MeasureDuration(_ => callbackInvoked = true);

        act.Should().Throw<InvalidOperationException>("the exception must propagate to the caller");
        callbackInvoked.Should().BeFalse(
            "the callback is placed after action() with no try/finally, " +
            "so it is never reached when the action throws");
    }

    [Fact]
    public void MeasureDuration_FuncWithOut_ShouldReturnResultAndDuration()
    {
        Func<int> func = () => 42;

        var result = func.MeasureDuration(out var duration);

        result.Should().Be(42);
        duration.Should().BeGreaterThanOrEqualTo(TimeSpan.Zero);
    }

    [Fact]
    public void MeasureDuration_FuncCallback_ShouldCaptureDurationAndResult()
    {
        TimeSpan capturedDuration = TimeSpan.Zero;
        var capturedResult = string.Empty;
        Func<string> func = () => "done";

        var result = func.MeasureDuration((duration, value) =>
        {
            capturedDuration = duration;
            capturedResult = value;
        });

        result.Should().Be("done");
        capturedResult.Should().Be("done");
        capturedDuration.Should().BeGreaterThanOrEqualTo(TimeSpan.Zero);
    }

    [Fact]
    public void MeasureDuration_FuncWithInputOut_ShouldReturnMappedValue()
    {
        Func<int, int> func = value => value * 2;

        var result = func.MeasureDuration(21, out var duration);

        result.Should().Be(42);
        duration.Should().BeGreaterThanOrEqualTo(TimeSpan.Zero);
    }

    [Fact]
    public void MeasureDuration_FuncWithInputCallback_ShouldInvokeCallback()
    {
        TimeSpan capturedDuration = TimeSpan.Zero;
        var capturedResult = 0;

        var result = ((Func<int, int>)(value => value + 1)).MeasureDuration(41, (duration, value) =>
        {
            capturedDuration = duration;
            capturedResult = value;
        });

        result.Should().Be(42);
        capturedResult.Should().Be(42);
        capturedDuration.Should().BeGreaterThanOrEqualTo(TimeSpan.Zero);
    }

    [Fact]
    public async Task MeasureDurationAsync_Func_ShouldReturnResult()
    {
        var result = await ((Func<int>)(() => 42)).MeasureDurationAsync();

        result.Should().Be(42);
    }

    [Fact]
    public async Task MeasureDurationAsync_FuncWithInput_ShouldReturnResult()
    {
        var result = await ((Func<int, int>)(value => value * 2)).MeasureDurationAsync(21);

        result.Should().Be(42);
    }

    // ─────────────────────────────────────────────────────────────────
    // Additional edge cases
    // ─────────────────────────────────────────────────────────────────

    [Fact]
    public void MeasureDuration_ActionWithNullCallback_ShouldNotThrow()
    {
        Action action = () => { };

        var act = () => action.MeasureDuration((Action<TimeSpan>?)null!);

        act.Should().NotThrow();
    }

    [Fact]
    public void MeasureDuration_WithVeryFastAction_ShouldReturnNonNegativeDuration()
    {
        Action action = () => { /* no-op */ };

        action.MeasureDuration(out var duration);

        duration.Should().BeGreaterThanOrEqualTo(TimeSpan.Zero);
    }

    [Fact]
    public void MeasureDuration_FuncWithInput_ShouldPassInputCorrectly()
    {
        Func<string, int> func = s => s.Length;

        var result = func.MeasureDuration("hello", out var duration);

        result.Should().Be(5);
        duration.Should().BeGreaterThanOrEqualTo(TimeSpan.Zero);
    }

    [Fact]
    public async Task MeasureDurationAsync_WithCallback_ShouldInvokeCallback()
    {
        TimeSpan capturedDuration = TimeSpan.Zero;
        int capturedResult = 0;

        var result = await ((Func<int>)(() => 99)).MeasureDurationAsync((d, r) =>
        {
            capturedDuration = d;
            capturedResult = r;
        });

        result.Should().Be(99);
        capturedResult.Should().Be(99);
        capturedDuration.Should().BeGreaterThanOrEqualTo(TimeSpan.Zero);
    }

    [Fact]
    public void MeasureDuration_FuncWithNullCallback_ShouldNotThrow()
    {
        Func<int> func = () => 42;

        var result = func.MeasureDuration((Action<TimeSpan, int>?)null!);

        result.Should().Be(42);
    }

    [Fact]
    public async Task MeasureDurationAsync_FuncWithInputAndCallback_ShouldInvokeCallback()
    {
        TimeSpan capturedDuration = TimeSpan.Zero;
        int capturedResult = 0;

        var result = await ((Func<int, int>)(x => x * 3)).MeasureDurationAsync(7, (d, r) =>
        {
            capturedDuration = d;
            capturedResult = r;
        });

        result.Should().Be(21);
        capturedResult.Should().Be(21);
        capturedDuration.Should().BeGreaterThanOrEqualTo(TimeSpan.Zero);
    }

    [Fact]
    public async Task MeasureDurationAsync_FuncWithInputAndNullCallback_ShouldNotThrow()
    {
        var result = await ((Func<string, int>)(s => s.Length)).MeasureDurationAsync("test", null);

        result.Should().Be(4);
    }
}
