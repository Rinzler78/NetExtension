using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rinlzer78.NetExtension.Dates
{
    public struct TimeSlot
    {
        public TimeSlot(DateTime start, DateTime end)
        {
            Start = start;
            End = end;
            Duration = end - start;
        }

        public DateTime Start { get; }
        public DateTime End { get; }
        public TimeSpan Duration { get; }

        public override string ToString()
        {
            return $"{Start} => {End} ({Duration})";
        }
    }

    public static class DatesHelper
    {
        public static TimeSlot[] ToTimeSlots(DateTime startDate, DateTime endDate, uint daysSlotDuration = 0)
        {
            var list = new List<TimeSlot>();

            if (endDate > startDate)
            {
                var start = startDate;
                DateTime end = default;

                do
                {
                    end = start.AddDays(daysSlotDuration);
                    end = end > endDate ? endDate : end;

                    list.Add(new TimeSlot(start, end));

                    start = end.AddSeconds(1);
                }
                while (end < endDate);
            }

            return list.ToArray();
        }

        public static async Task<ReturnType[]> RunTask<ReturnType>(this TimeSlot[] timeSlots, Func<TimeSlot, Task<ReturnType>> func)
        {
            var tasks = timeSlots.Select(arg => func(arg));

            var results = await Task.WhenAll(tasks);

            return results;
        }
    }
}
