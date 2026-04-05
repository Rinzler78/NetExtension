using Rinzler78.NetExtension.Objects;

namespace Rinzler78.NetExtension.Tests.Objects;

[Trait("Category", "Unit")]
public class UpdatablePropertyExtensionTests
{
    // ─────────────────────────────────────────────────────────────────
    // GetAll
    // ─────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetAll_WithEmptyArray_CompletesWithoutError()
    {
        // Should not throw
        await System.Array.Empty<UpdatableProperty>().GetAll();
    }

    [Fact]
    public async Task GetAll_WithMultipleProperties_AllInitialized()
    {
        var p1 = new UpdatableProperty<int>(() => Task.FromResult(1));
        var p2 = new UpdatableProperty<int>(() => Task.FromResult(2));
        var p3 = new UpdatableProperty<int>(() => Task.FromResult(3));
        var props = new UpdatableProperty[] { p1, p2, p3 };

        await props.GetAll();

        p1.Property.Should().Be(1);
        p2.Property.Should().Be(2);
        p3.Property.Should().Be(3);
    }

    // ─────────────────────────────────────────────────────────────────
    // UpdateAll
    // ─────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UpdateAll_WithEmptyArray_CompletesWithoutError()
    {
        // Should not throw
        await System.Array.Empty<UpdatableProperty>().UpdateAll();
    }

    [Fact]
    public async Task UpdateAll_WithMultipleProperties_AllRefreshed()
    {
        int counter = 0;
        var p1 = new UpdatableProperty<int>(() => Task.FromResult(Interlocked.Increment(ref counter)));
        var p2 = new UpdatableProperty<int>(() => Task.FromResult(Interlocked.Increment(ref counter)));
        var props = new UpdatableProperty[] { p1, p2 };

        // First get to initialize
        await props.GetAll();
        int afterFirst = counter;

        // Force update — each property re-invokes its factory
        await props.UpdateAll();

        counter.Should().BeGreaterThan(afterFirst);
    }
}
