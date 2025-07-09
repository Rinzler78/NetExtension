using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rinzler78.NetExtension.Dates;

public static class DatesHelper
{
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
                    start = end.AddSeconds(1);
                }
            }
        }
        return list.ToArray();
    }

    public static async Task<ReturnType[]> RunTask<ReturnType>(this TimeSlot[] timeSlots,
        Func<TimeSlot, Task<ReturnType>> func)
    {
        var tasks = timeSlots.Select(arg => func(arg));

        var results = await Task.WhenAll(tasks).ConfigureAwait(false);

        return results;
    }

    public static TimeSpan ElapsedUtc(this DateTime date) => DateTime.UtcNow - date;
}