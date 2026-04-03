using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Rinzler78.NetExtension.Dates;
using Xunit;

namespace Rinzler78.NetExtension.Tests.Dates;

[Trait("Category", "Unit")]
public class DatesHelperTests
{
    [Fact]
    public void ToTimeSlots_ValidDateRange_ShouldReturnCorrectSlots()
    {
        // Arrange
        var startDate = new DateTime(2023, 1, 1);
        var endDate = new DateTime(2023, 1, 3);

        // Act
        var result = DatesHelper.ToTimeSlots(startDate, endDate, 1);

        // Assert
        Assert.Equal(2, result.Length);
        Assert.Equal(startDate, result[0].Start);
        Assert.Equal(new DateTime(2023, 1, 2), result[0].End);
        // Slot 1 starts exactly TimeSlotSeparationSeconds after the previous slot ended.
        Assert.Equal(result[0].End.AddSeconds(DatesHelper.TimeSlotSeparationSeconds), result[1].Start);
        Assert.Equal(endDate, result[1].End);
    }

    [Fact]
    public void ToTimeSlots_StartDateAfterEndDate_ShouldReturnEmptyArray()
    {
        // Arrange
        var startDate = new DateTime(2023, 1, 3);
        var endDate = new DateTime(2023, 1, 1);

        // Act
        var result = DatesHelper.ToTimeSlots(startDate, endDate, 1);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void ToTimeSlots_SameDates_ShouldReturnEmptyArray()
    {
        // Arrange
        var date = new DateTime(2023, 1, 1);

        // Act
        var result = DatesHelper.ToTimeSlots(date, date, 1);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void ToTimeSlots_ZeroDaysDuration_ShouldCreateSingleSlot()
    {
        // Arrange
        var startDate = new DateTime(2023, 1, 1);
        var endDate = new DateTime(2023, 1, 2);

        // Act
        var result = DatesHelper.ToTimeSlots(startDate, endDate, 0);

        // Assert
        Assert.Single(result);
        Assert.Equal(startDate, result[0].Start);
        Assert.Equal(endDate, result[0].End);
    }

    [Fact]
    public void ToTimeSlots_LargeDaysDuration_ShouldCreateSingleSlot()
    {
        // Arrange
        var startDate = new DateTime(2023, 1, 1);
        var endDate = new DateTime(2023, 1, 3);

        // Act
        var result = DatesHelper.ToTimeSlots(startDate, endDate, 10); // 10 days > 2 days range

        // Assert
        Assert.Single(result);
        Assert.Equal(startDate, result[0].Start);
        Assert.Equal(endDate, result[0].End);
    }

    [Fact]
    public void ToTimeSlots_MultipleSlots_ShouldHaveCorrectTiming()
    {
        // Arrange
        var startDate = new DateTime(2023, 1, 1, 10, 0, 0);
        var endDate = new DateTime(2023, 1, 4, 15, 30, 0);

        // Act
        var result = DatesHelper.ToTimeSlots(startDate, endDate, 1);

        // Assert
        Assert.Equal(4, result.Length);

        // First slot: Jan 1 10:00 to Jan 2 10:00
        Assert.Equal(new DateTime(2023, 1, 1, 10, 0, 0), result[0].Start);
        Assert.Equal(new DateTime(2023, 1, 2, 10, 0, 0), result[0].End);

        // Second slot starts exactly TimeSlotSeparationSeconds after slot 0 ends.
        Assert.Equal(result[0].End.AddSeconds(DatesHelper.TimeSlotSeparationSeconds), result[1].Start);
        Assert.Equal(result[1].Start.AddDays(1), result[1].End);

        // Third slot starts exactly TimeSlotSeparationSeconds after slot 1 ends.
        Assert.Equal(result[1].End.AddSeconds(DatesHelper.TimeSlotSeparationSeconds), result[2].Start);
        Assert.Equal(result[2].Start.AddDays(1), result[2].End);

        // Fourth slot: starts TimeSlotSeparationSeconds after slot 2 ends, finishes at endDate.
        Assert.Equal(result[2].End.AddSeconds(DatesHelper.TimeSlotSeparationSeconds), result[3].Start);
        Assert.Equal(new DateTime(2023, 1, 4, 15, 30, 0), result[3].End);
    }

    [Fact]
    public async Task RunTask_ValidTimeSlots_ShouldExecuteAllTasks()
    {
        // Arrange
        var timeSlots = new[]
        {
            new TimeSlot(new DateTime(2023, 1, 1), new DateTime(2023, 1, 2)),
            new TimeSlot(new DateTime(2023, 1, 2), new DateTime(2023, 1, 3)),
            new TimeSlot(new DateTime(2023, 1, 3), new DateTime(2023, 1, 4))
        };

        var executionCounter = 0;
        async Task<int> TestFunction(TimeSlot slot)
        {
            await Task.Delay(1); // Simulate async work
            return Interlocked.Increment(ref executionCounter);
        }

        // Act
        var results = await timeSlots.RunTask(TestFunction);

        // Assert
        Assert.Equal(3, results.Length);
        Assert.Equal(new[] { 1, 2, 3 }, results.OrderBy(x => x).ToArray());
    }

    [Fact]
    public async Task RunTask_EmptyTimeSlots_ShouldReturnEmptyArray()
    {
        // Arrange
        var timeSlots = new TimeSlot[0];

        async Task<string> TestFunction(TimeSlot slot)
        {
            await Task.Delay(1);
            return "test";
        }

        // Act
        var results = await timeSlots.RunTask(TestFunction);

        // Assert
        Assert.Empty(results);
    }

    [Fact]
    public async Task RunTask_TaskThrowsException_ShouldPropagateException()
    {
        // Arrange
        var timeSlots = new[]
        {
            new TimeSlot(new DateTime(2023, 1, 1), new DateTime(2023, 1, 2))
        };

        async Task<string> ThrowingFunction(TimeSlot slot)
        {
            await Task.Delay(1);
            throw new InvalidOperationException("Test exception");
        }

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => timeSlots.RunTask(ThrowingFunction));
    }

    [Fact]
    public void ElapsedUtc_PastDate_ShouldReturnPositiveTimeSpan()
    {
        // Arrange
        var pastDate = DateTime.UtcNow.AddHours(-1);

        // Act
        var elapsed = pastDate.ElapsedUtc();

        // Assert
        Assert.True(elapsed.TotalMinutes > 45); // Should be close to 1 hour with generous tolerance
        Assert.True(elapsed.TotalMinutes < 75); // But not too much more (allowing for test execution time and CI load)
    }

    [Fact]
    public void ElapsedUtc_FutureDate_ShouldReturnNegativeTimeSpan()
    {
        // Arrange
        var futureDate = DateTime.UtcNow.AddHours(1);

        // Act
        var elapsed = futureDate.ElapsedUtc();

        // Assert
        Assert.True(elapsed.TotalMinutes < -45); // Should be close to -1 hour with generous tolerance
        Assert.True(elapsed.TotalMinutes > -75); // But not too much less (allowing for test execution time and CI load)
    }

    [Fact]
    public void ElapsedUtc_CurrentTime_ShouldReturnSmallTimeSpan()
    {
        // Arrange
        var now = DateTime.UtcNow;

        // Act
        var elapsed = now.ElapsedUtc();

        // Assert
        Assert.True(System.Math.Abs(elapsed.TotalMilliseconds) < 1000); // Should be very small, increased tolerance for CI environments
    }
}
