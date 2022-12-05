using System;
using System.Runtime.InteropServices;

namespace Rinzler78.NetExtension.Dates;

[StructLayout(LayoutKind.Auto)]
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

    public readonly override string ToString()
    {
        return $"{Start} => {End} ({Duration})";
    }
}