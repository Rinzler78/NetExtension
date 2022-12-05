using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Rinzler78.NetExtension.Measure;

public static class PerformanceHelper
{
    public static void MeasureDuration(this Action action, out TimeSpan duration)
    {
        var timer = new Stopwatch();

        timer.Start();

        action();

        duration = timer.Elapsed;
    }

    public static void MeasureDuration(this Action action, Action<TimeSpan> durationAction = null)
    {
        action.MeasureDuration(out var duration);

        durationAction?.Invoke(duration);
    }

    public static ReturnType MeasureDuration<ReturnType>(this Func<ReturnType> func, out TimeSpan duration)
    {
        var timer = new Stopwatch();

        timer.Start();

        var result = func();

        duration = timer.Elapsed;

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
        var timer = new Stopwatch();

        timer.Start();

        var result = func(input);

        duration = timer.Elapsed;

        return result;
    }

    public static ReturnType MeasureDuration<InType, ReturnType>(this Func<InType, ReturnType> func, InType input,
        Action<TimeSpan, ReturnType> durationAction = null)
    {
        var timer = new Stopwatch();

        timer.Start();

        var result = func(input);

        durationAction?.Invoke(timer.Elapsed, result);

        return result;
    }

    public static async Task<ReturnType> MeasureDurationAsync<ReturnType>(this Func<ReturnType> func,
        Action<TimeSpan, ReturnType> durationAction = null)
    {
        return await Task.Run(() => MeasureDuration(func, durationAction)).ConfigureAwait(false);
    }

    public static async Task<ReturnType> MeasureDurationAsync<InType, ReturnType>(this Func<InType, ReturnType> func,
        InType input, Action<TimeSpan, ReturnType> durationAction = null)
    {
        return await Task.Run(() => MeasureDuration(func, input, durationAction)).ConfigureAwait(false);
    }
}