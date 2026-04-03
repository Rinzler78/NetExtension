using Rinzler78.NetExtension.Objects;

namespace Rinzler78.NetExtension.Tests.Objects;

[Trait("Category", "Unit")]
public class UpdatablePropertyTests
{
    [Fact]
    public async Task Get_ShouldPopulatePropertyOnlyOnceUntilForced()
    {
        var calls = 0;
        var property = new UpdatableProperty<int>(async () =>
        {
            await Task.Delay(1);
            return Interlocked.Increment(ref calls);
        });

        var first = await property.Get();
        var second = await property.Get();
        var forced = await property.Get(force: true);

        first.Should().Be(1);
        second.Should().Be(1);
        forced.Should().Be(2);
        property.Property.Should().Be(2);
    }

    [Fact]
    public async Task Update_ShouldReuseInFlightTask()
    {
        var calls = 0;
        var gate = new TaskCompletionSource<int>();
        var property = new UpdatableProperty<int>(async () =>
        {
            Interlocked.Increment(ref calls);
            return await gate.Task;
        });

        var update1 = property.Update();
        var update2 = property.Update();
        gate.SetResult(42);
        await Task.WhenAll(update1, update2);

        calls.Should().Be(1);
        property.Property.Should().Be(42);
    }

    [Fact]
    public async Task ExtensionMethods_ShouldGetAndUpdateAll()
    {
        var property1 = new UpdatableProperty<int>(() => Task.FromResult(1));
        var property2 = new UpdatableProperty<int>(() => Task.FromResult(2));
        var properties = new UpdatableProperty[] { property1, property2 };

        await properties.GetAll();
        await properties.UpdateAll();

        property1.Property.Should().Be(1);
        property2.Property.Should().Be(2);
    }

    [Fact]
    public void Constructor_WithNullDelegate_ShouldThrow()
    {
        Action act = () => _ = new UpdatableProperty<int>(null!);

        act.Should().Throw<ArgumentNullException>();
    }
}
