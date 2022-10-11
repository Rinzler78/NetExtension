using System;
using System.Threading.Tasks;

namespace Rinzler78.NetExtension.Measure
{
    public static class PerformanceHelper
    {
        public static void MeasureDuration(this Action action, Action<TimeSpan> duration = null)
        {
            var startDate = DateTime.UtcNow;

            action();

            if (duration != null)
                duration(DateTime.UtcNow - startDate);
        }

        public static ReturnType MeasureDuration<ReturnType>(this Func<ReturnType> func, Action<TimeSpan, ReturnType> duration = null)
        {
            var startDate = DateTime.UtcNow;

            ReturnType result = func();

            if (duration != null)
                duration(DateTime.UtcNow - startDate, result);

            return result;
        }

        public static ReturnType MeasureDuration<InType, ReturnType>(this Func<InType, ReturnType> func, InType input, Action<TimeSpan, ReturnType> duration = null)
        {
            var startDate = DateTime.UtcNow;

            ReturnType result = func(input);

            if (duration != null)
                duration(DateTime.UtcNow - startDate, result);

            return result;
        }

        public static async Task<ReturnType> MeasureDurationAsync<ReturnType>(this Func<ReturnType> func, Action<TimeSpan, ReturnType> duration = null)
        {
            return await Task.Run(() => MeasureDuration(func, duration));
        }

        public static async Task<ReturnType> MeasureDurationAsync<InType, ReturnType>(this Func<InType, ReturnType> func, InType input, Action<TimeSpan, ReturnType> duration = null)
        {
            return await Task.Run(() => MeasureDuration(func, input, duration));
        }
    }
}