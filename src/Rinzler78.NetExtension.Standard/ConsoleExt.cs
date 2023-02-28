using System;
using System.Diagnostics;

namespace Rinzler78.NetExtension;

public static class ConsoleExt
{
    public static readonly object Locker = new();

    public static void ConsoleWriteLine(this string str, ConsoleColor? backgroundColor = null, ConsoleColor? foregroundColor = null)
    {
        if (backgroundColor != null || foregroundColor != null)
        {
            lock (Locker)
            {
                var previousForegroundColor = Console.ForegroundColor;
                var previousBackgroundColor = Console.BackgroundColor;

                Console.ForegroundColor = foregroundColor ?? previousForegroundColor;
                Console.BackgroundColor = backgroundColor ?? previousBackgroundColor;

                Console.WriteLine(str);

                Console.ForegroundColor = previousForegroundColor;
                Console.BackgroundColor = previousBackgroundColor;
            }
        }
        else
            Console.WriteLine(str);
    }
}

