using System;
using System.Runtime.InteropServices;

namespace Rinzler78.NetExtension.Dates;

/// <summary>
/// Represents a time interval defined by a start and end date.
/// </summary>
[StructLayout(LayoutKind.Auto)]
public struct TimeSlot : IEquatable<TimeSlot>
{
    /// <summary>
    /// Initializes a new <see cref="TimeSlot"/> with the specified start and end dates.
    /// </summary>
    /// <param name="start">The start date of the time slot.</param>
    /// <param name="end">The end date of the time slot.</param>
    public TimeSlot(DateTime start, DateTime end)
    {
        Start = start;
        End = end;
        Duration = end - start;
    }

    /// <summary>
    /// Gets the start date of the time slot.
    /// </summary>
    public DateTime Start { get; }

    /// <summary>
    /// Gets the end date of the time slot.
    /// </summary>
    public DateTime End { get; }

    /// <summary>
    /// Gets the duration of the time slot.
    /// </summary>
    public TimeSpan Duration { get; }

    public readonly bool Equals(TimeSlot other)
    {
        return Start == other.Start && End == other.End;
    }

    public override readonly bool Equals(object? obj)
    {
        return obj is TimeSlot other && Equals(other);
    }

    public override readonly int GetHashCode()
    {
        return HashCode.Combine(Start, End);
    }

    public static bool operator ==(TimeSlot left, TimeSlot right) => left.Equals(right);
    public static bool operator !=(TimeSlot left, TimeSlot right) => !left.Equals(right);

    public override readonly string ToString()
    {
        return $"{Start} => {End} ({Duration})";
    }
}
