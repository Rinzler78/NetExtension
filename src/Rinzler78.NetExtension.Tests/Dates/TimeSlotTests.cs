using Rinzler78.NetExtension.Dates;

namespace Rinzler78.NetExtension.Tests.Dates;

[Trait("Category", "Unit")]
public class TimeSlotTests
{
    [Fact]
    public void TimeSlot_ShouldExposeStartEndDurationAndString()
    {
        var start = new DateTime(2024, 1, 1, 12, 0, 0, DateTimeKind.Utc);
        var end = start.AddMinutes(30);

        var slot = new TimeSlot(start, end);

        slot.Start.Should().Be(start);
        slot.End.Should().Be(end);
        slot.Duration.Should().Be(TimeSpan.FromMinutes(30));
        slot.ToString().Should().Contain("=>");
    }

    [Fact]
    public void TimeSlot_ZeroDuration_StartEqualsEnd_DurationShouldBeZero()
    {
        var point = new DateTime(2024, 6, 15, 9, 0, 0, DateTimeKind.Utc);

        var slot = new TimeSlot(point, point);

        slot.Start.Should().Be(point);
        slot.End.Should().Be(point);
        slot.Duration.Should().Be(TimeSpan.Zero,
            "when Start == End the duration is end - start = 0");
    }

    [Fact]
    public void TimeSlot_NegativeDuration_EndBeforeStart_DurationShouldBeNegative()
    {
        // Documents actual behaviour: Duration = End - Start, which is negative
        // when End < Start.  The struct performs no validation and does not throw.
        var start = new DateTime(2024, 1, 1, 12, 0, 0, DateTimeKind.Utc);
        var end = start.AddHours(-1); // one hour before start

        var slot = new TimeSlot(start, end);

        slot.Duration.Should().BeLessThan(TimeSpan.Zero,
            "TimeSlot does not validate the order of Start/End; " +
            "Duration = End - Start is negative when End < Start");
        slot.Duration.Should().Be(TimeSpan.FromHours(-1));
    }

    [Fact]
    public void TimeSlot_Equals_SameStartAndEnd_ShouldBeEqual()
    {
        var start = new DateTime(2024, 1, 1, 12, 0, 0, DateTimeKind.Utc);
        var end = start.AddMinutes(30);

        var slot1 = new TimeSlot(start, end);
        var slot2 = new TimeSlot(start, end);

        slot1.Equals(slot2).Should().BeTrue();
        (slot1 == slot2).Should().BeTrue();
        (slot1 != slot2).Should().BeFalse();
        slot1.GetHashCode().Should().Be(slot2.GetHashCode());
    }

    [Fact]
    public void TimeSlot_Equals_DifferentValues_ShouldNotBeEqual()
    {
        var start = new DateTime(2024, 1, 1, 12, 0, 0, DateTimeKind.Utc);
        var slot1 = new TimeSlot(start, start.AddMinutes(30));
        var slot2 = new TimeSlot(start, start.AddMinutes(60));

        slot1.Equals(slot2).Should().BeFalse();
        (slot1 != slot2).Should().BeTrue();
        (slot1 == slot2).Should().BeFalse();
    }

    [Fact]
    public void TimeSlot_Equals_ObjectOverload_ShouldWork()
    {
        var start = new DateTime(2024, 1, 1, 12, 0, 0, DateTimeKind.Utc);
        var end = start.AddMinutes(30);
        var slot = new TimeSlot(start, end);

        slot.Equals((object)new TimeSlot(start, end)).Should().BeTrue();
        slot.Equals((object)"not a timeslot").Should().BeFalse();
        slot.Equals(null).Should().BeFalse();
    }

    [Fact]
    public void TimeSlot_BoundaryValues_MinMaxDateTime_ShouldNotThrow()
    {
        // Verify that DateTime boundary values are accepted without overflow exceptions.
        var slotMinToMax = new TimeSlot(DateTime.MinValue, DateTime.MaxValue);
        var slotMaxToMin = new TimeSlot(DateTime.MaxValue, DateTime.MinValue);

        slotMinToMax.Start.Should().Be(DateTime.MinValue);
        slotMinToMax.End.Should().Be(DateTime.MaxValue);
        slotMinToMax.Duration.Should().BeGreaterThan(TimeSpan.Zero);

        slotMaxToMin.Start.Should().Be(DateTime.MaxValue);
        slotMaxToMin.End.Should().Be(DateTime.MinValue);
        slotMaxToMin.Duration.Should().BeLessThan(TimeSpan.Zero);
    }
}
