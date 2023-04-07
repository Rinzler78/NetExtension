using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Rinzler78.NetExtension.Measure;

public static class PerformanceHelper
{
    public static void MeasureDuration(this Action action, out TimeSpan duration)
    {
        var startTime = Stopwatch.GetTimestamp();

        action();

        duration = Stopwatch.GetElapsedTime(startTime);
    }

    public static void MeasureDuration(this Action action, Action<TimeSpan>? durationAction = null)
    {
        action.MeasureDuration(out var duration);

        durationAction?.Invoke(duration);
    }

    public static ReturnType MeasureDuration<ReturnType>(this Func<ReturnType> func, out TimeSpan duration)
    {
        var startTime = Stopwatch.GetTimestamp();

        var result = func();

        duration = Stopwatch.GetElapsedTime(startTime);

        return result;
    }

    public static ReturnType MeasureDuration<ReturnType>(this Func<ReturnType> func,
        Action<TimeSpan, ReturnType>? durationAction = null)
    {
        var result = func.MeasureDuration(out var duration);

        durationAction?.Invoke(duration, result);

        return result;
    }

    public static ReturnType MeasureDuration<InType, ReturnType>(this Func<InType, ReturnType> func, InType input,
        out TimeSpan duration)
    {
        var startTime = Stopwatch.GetTimestamp();

        var result = func(input);

        duration = Stopwatch.GetElapsedTime(startTime);

        return result;
    }

    public static ReturnType MeasureDuration<InType, ReturnType>(this Func<InType, ReturnType> func, InType input,
        Action<TimeSpan, ReturnType>? durationAction = null)
    {
        var startTime = Stopwatch.GetTimestamp();

        var result = func(input);

        durationAction?.Invoke(Stopwatch.GetElapsedTime(startTime), result);

        return result;
    }

    public static async Task<ReturnType> MeasureDurationAsync<ReturnType>(this Func<ReturnType> func,
        Action<TimeSpan, ReturnType>? durationAction = null)
    {
        return await Task.Run(() => MeasureDuration(func, durationAction)).ConfigureAwait(false);
    }

    public static async Task<ReturnType> MeasureDurationAsync<InType, ReturnType>(this Func<InType, ReturnType> func,
        InType input, Action<TimeSpan, ReturnType>? durationAction = null)
    {
        return await Task.Run(() => MeasureDuration(func, input, durationAction)).ConfigureAwait(false);
    }
}