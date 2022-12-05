using System;

namespace Rinzler78.NetExtension.Dates;

[System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Auto)]
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

    public override readonly string ToString()
    {
        return $"{Start} => {End} ({Duration})";
    }
}