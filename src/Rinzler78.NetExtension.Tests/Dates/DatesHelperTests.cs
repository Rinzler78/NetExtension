using System;
using System.Linq;
using System.Threading.Tasks;
using Rinzler78.NetExtension.Dates;
using Xunit;

namespace Rinzler78.NetExtension.Tests.Dates;

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
        Assert.Equal(new DateTime(2023, 1, 2, 0, 0, 1), result[1].Start);
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

        // Second slot: Jan 2 10:00:01 to Jan 3 10:00:01
        Assert.Equal(new DateTime(2023, 1, 2, 10, 0, 1), result[1].Start);
        Assert.Equal(new DateTime(2023, 1, 3, 10, 0, 1), result[1].End);

        // Third slot: Jan 3 10:00:02 to Jan 4 10:00:02
        Assert.Equal(new DateTime(2023, 1, 3, 10, 0, 2), result[2].Start);
        Assert.Equal(new DateTime(2023, 1, 4, 10, 0, 2), result[2].End);

        // Fourth slot: Jan 4 10:00:03 to Jan 4 15:30 (end date)
        Assert.Equal(new DateTime(2023, 1, 4, 10, 0, 3), result[3].Start);
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
            return ++executionCounter;
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
        Assert.True(elapsed.TotalMinutes > 50); // Should be close to 1 hour
        Assert.True(elapsed.TotalMinutes < 70); // But not too much more (allowing for test execution time)
    }

    [Fact]
    public void ElapsedUtc_FutureDate_ShouldReturnNegativeTimeSpan()
    {
        // Arrange
        var futureDate = DateTime.UtcNow.AddHours(1);

        // Act
        var elapsed = futureDate.ElapsedUtc();

        // Assert
        Assert.True(elapsed.TotalMinutes < -50); // Should be close to -1 hour
        Assert.True(elapsed.TotalMinutes > -70); // But not too much less
    }

    [Fact]
    public void ElapsedUtc_CurrentTime_ShouldReturnSmallTimeSpan()
    {
        // Arrange
        var now = DateTime.UtcNow;

        // Act
        var elapsed = now.ElapsedUtc();

        // Assert
        Assert.True(System.Math.Abs(elapsed.TotalMilliseconds) < 100); // Should be very small
    }
}
