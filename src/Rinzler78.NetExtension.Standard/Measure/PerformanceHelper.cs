using System;
using System.Threading.Tasks;

namespace Rinzler78.NetExtension.Measure;

public static class PerformanceHelper
{
    public static void MeasureDuration(this Action action, out TimeSpan duration)
    {
        var startDate = DateTime.UtcNow;

        action();

        duration = DateTime.UtcNow - startDate;
    }

    public static void MeasureDuration(this Action action, Action<TimeSpan> durationAction = null)
    {
        action.MeasureDuration(out var duration);

        durationAction?.Invoke(duration);
    }

    public static ReturnType MeasureDuration<ReturnType>(this Func<ReturnType> func, out TimeSpan duration)
    {
        var startDate = DateTime.UtcNow;

        var result = func();

        duration = DateTime.UtcNow - startDate;

        return result;
    }

    public static ReturnType MeasureDuration<ReturnType>(this Func<ReturnType> func,
        Action<TimeSpan, ReturnType> durationAction = null)
    {
        var result = func.MeasureDuration(out var duration);

        durationAction?.Invoke(duration, result);

        return result;
    }

    public static ReturnType MeasureDuration<InType, ReturnType>(this Func<InType, ReturnType> func, InType input,
        out TimeSpan duration)
    {
        var startDate = DateTime.UtcNow;

        var result = func(input);

        duration = DateTime.UtcNow - startDate;

        return result;
    }

    public static ReturnType MeasureDuration<InType, ReturnType>(this Func<InType, ReturnType> func, InType input,
        Action<TimeSpan, ReturnType> durationAction = null)
    {
        var startDate = DateTime.UtcNow;

        var result = func(input);

        durationAction?.Invoke(DateTime.UtcNow - startDate, result);

        return result;
    }

    public static async Task<ReturnType> MeasureDurationAsync<ReturnType>(this Func<ReturnType> func,
        Action<TimeSpan, ReturnType> durationAction = null)
    {
        return await Task.Run(() => MeasureDuration(func, durationAction));
    }

    public static async Task<ReturnType> MeasureDurationAsync<InType, ReturnType>(this Func<InType, ReturnType> func,
        InType input, Action<TimeSpan, ReturnType> durationAction = null)
    {
        return await Task.Run(() => MeasureDuration(func, input, durationAction));
    }
}