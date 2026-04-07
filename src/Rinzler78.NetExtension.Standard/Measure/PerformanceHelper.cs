using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Rinzler78.NetExtension.Measure;

/// <summary>
/// Provides extension methods to measure the execution duration of actions and functions.
/// </summary>
public static class PerformanceHelper
{
    /// <summary>
    /// Executes the action and returns its elapsed duration via an out parameter.
    /// </summary>
    /// <param name="action">The action to measure.</param>
    /// <param name="duration">The elapsed duration of the action.</param>
    public static void MeasureDuration(this Action action, out TimeSpan duration)
    {
        var startTime = Stopwatch.GetTimestamp();

        action();

        duration = Stopwatch.GetElapsedTime(startTime);
    }

    /// <summary>
    /// Executes the action and invokes the callback with the elapsed duration.
    /// </summary>
    /// <param name="action">The action to measure.</param>
    /// <param name="durationAction">Optional callback receiving the elapsed duration.</param>
    public static void MeasureDuration(this Action action, Action<TimeSpan>? durationAction = null)
    {
        action.MeasureDuration(out var duration);

        durationAction?.Invoke(duration);
    }

    /// <summary>
    /// Executes the function, returns its result, and outputs the elapsed duration.
    /// </summary>
    /// <typeparam name="ReturnType">The return type of the function.</typeparam>
    /// <param name="func">The function to measure.</param>
    /// <param name="duration">The elapsed duration of the function.</param>
    /// <returns>The result of the function.</returns>
    public static ReturnType MeasureDuration<ReturnType>(this Func<ReturnType> func, out TimeSpan duration)
    {
        var startTime = Stopwatch.GetTimestamp();

        var result = func();

        duration = Stopwatch.GetElapsedTime(startTime);

        return result;
    }

    /// <summary>
    /// Executes the function and invokes the callback with the elapsed duration and result.
    /// </summary>
    /// <typeparam name="ReturnType">The return type of the function.</typeparam>
    /// <param name="func">The function to measure.</param>
    /// <param name="durationAction">Optional callback receiving the elapsed duration and result.</param>
    /// <returns>The result of the function.</returns>
    public static ReturnType MeasureDuration<ReturnType>(this Func<ReturnType> func,
        Action<TimeSpan, ReturnType>? durationAction = null)
    {
        var result = func.MeasureDuration(out var duration);

        durationAction?.Invoke(duration, result);

        return result;
    }

    /// <summary>
    /// Executes the function with the given input, returns its result, and outputs the elapsed duration.
    /// </summary>
    /// <typeparam name="InType">The input parameter type.</typeparam>
    /// <typeparam name="ReturnType">The return type of the function.</typeparam>
    /// <param name="func">The function to measure.</param>
    /// <param name="input">The input to pass to the function.</param>
    /// <param name="duration">The elapsed duration of the function.</param>
    /// <returns>The result of the function.</returns>
    public static ReturnType MeasureDuration<InType, ReturnType>(this Func<InType, ReturnType> func, InType input,
        out TimeSpan duration)
    {
        var startTime = Stopwatch.GetTimestamp();

        var result = func(input);

        duration = Stopwatch.GetElapsedTime(startTime);

        return result;
    }

    /// <summary>
    /// Executes the function with the given input and invokes the callback with the elapsed duration and result.
    /// </summary>
    /// <typeparam name="InType">The input parameter type.</typeparam>
    /// <typeparam name="ReturnType">The return type of the function.</typeparam>
    /// <param name="func">The function to measure.</param>
    /// <param name="input">The input to pass to the function.</param>
    /// <param name="durationAction">Optional callback receiving the elapsed duration and result.</param>
    /// <returns>The result of the function.</returns>
    public static ReturnType MeasureDuration<InType, ReturnType>(this Func<InType, ReturnType> func, InType input,
        Action<TimeSpan, ReturnType>? durationAction = null)
    {
        var startTime = Stopwatch.GetTimestamp();

        var result = func(input);

        durationAction?.Invoke(Stopwatch.GetElapsedTime(startTime), result);

        return result;
    }

    /// <summary>
    /// Asynchronously executes the function and invokes the callback with the elapsed duration and result.
    /// </summary>
    /// <typeparam name="ReturnType">The return type of the function.</typeparam>
    /// <param name="func">The function to measure.</param>
    /// <param name="durationAction">Optional callback receiving the elapsed duration and result.</param>
    /// <returns>The result of the function.</returns>
    public static async Task<ReturnType> MeasureDurationAsync<ReturnType>(this Func<ReturnType> func,
        Action<TimeSpan, ReturnType>? durationAction = null)
    {
        return await Task.Run(() => MeasureDuration(func, durationAction)).ConfigureAwait(false);
    }

    /// <summary>
    /// Asynchronously executes the function with the given input and invokes the callback with the elapsed duration and result.
    /// </summary>
    /// <typeparam name="InType">The input parameter type.</typeparam>
    /// <typeparam name="ReturnType">The return type of the function.</typeparam>
    /// <param name="func">The function to measure.</param>
    /// <param name="input">The input to pass to the function.</param>
    /// <param name="durationAction">Optional callback receiving the elapsed duration and result.</param>
    /// <returns>The result of the function.</returns>
    public static async Task<ReturnType> MeasureDurationAsync<InType, ReturnType>(this Func<InType, ReturnType> func,
        InType input, Action<TimeSpan, ReturnType>? durationAction = null)
    {
        return await Task.Run(() => MeasureDuration(func, input, durationAction)).ConfigureAwait(false);
    }
}
