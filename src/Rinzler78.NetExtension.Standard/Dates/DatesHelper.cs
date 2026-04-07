using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rinzler78.NetExtension.Dates;

/// <summary>
/// Provides helper methods for date/time operations including time slot generation.
/// </summary>
public static class DatesHelper
{
    #region Constants

    /// <summary>
    /// Number of seconds to add between time slots to prevent overlap.
    /// </summary>
    public const int TimeSlotSeparationSeconds = 1;

    #endregion
    /// <summary>
    /// Splits a date range into consecutive time slots of the specified duration in days.
    /// </summary>
    /// <param name="startDate">The start of the date range.</param>
    /// <param name="endDate">The end of the date range.</param>
    /// <param name="daysSlotDuration">The duration of each slot in days. Zero means a single slot.</param>
    /// <returns>An array of time slots covering the date range.</returns>
    public static TimeSlot[] ToTimeSlots(DateTime startDate, DateTime endDate, uint daysSlotDuration = 0)
    {
        var list = new List<TimeSlot>();
        if (endDate > startDate)
        {
            if (daysSlotDuration == 0)
            {
                list.Add(new TimeSlot(startDate, endDate));
            }
            else
            {
                var start = startDate;
                while (start < endDate)
                {
                    var end = start.AddDays(daysSlotDuration);
                    if (end > endDate)
                    {
                        list.Add(new TimeSlot(start, endDate));
                        break;
                    }
                    list.Add(new TimeSlot(start, end));
                    start = end.AddSeconds(TimeSlotSeparationSeconds);
                }
            }
        }
        return list.ToArray();
    }

    /// <summary>
    /// Executes an asynchronous function for each time slot in parallel and returns all results.
    /// </summary>
    /// <typeparam name="ReturnType">The return type of the async function.</typeparam>
    /// <param name="timeSlots">The time slots to process.</param>
    /// <param name="func">The async function to execute for each time slot.</param>
    /// <returns>An array of results from all time slot executions.</returns>
    public static async Task<ReturnType[]> RunTask<ReturnType>(this TimeSlot[] timeSlots,
        Func<TimeSlot, Task<ReturnType>> func)
    {
        var tasks = timeSlots.Select(arg => func(arg));

        var results = await Task.WhenAll(tasks).ConfigureAwait(false);

        return results;
    }

    /// <summary>
    /// Returns the elapsed time between the specified date and the current UTC time.
    /// </summary>
    /// <param name="date">The reference date.</param>
    /// <returns>The elapsed time span.</returns>
    public static TimeSpan ElapsedUtc(this DateTime date) => DateTime.UtcNow - date;
}
