using System;
using System.Diagnostics;

namespace Rinzler78.NetExtension;

public static class ConsoleExt
{
    public static readonly ConsoleColor DefaultForegroundColor = Console.ForegroundColor;
    public static readonly ConsoleColor DefaultBackgroundColor = Console.BackgroundColor;

    public static readonly object Locker = new();

    public static void ConsoleWriteLine(this string str, ConsoleColor? backgroundColor = null, ConsoleColor? foregroundColor = null)
    {
        try
        {
            backgroundColor ??= DefaultBackgroundColor;
            foregroundColor ??= DefaultForegroundColor;

            if (backgroundColor != Console.BackgroundColor || foregroundColor != Console.ForegroundColor)
            {
                lock (Locker)
                {
                    Console.ForegroundColor = foregroundColor.Value;
                    Console.BackgroundColor = backgroundColor.Value;

                    Console.Write(str);

                    ResetDefaultConsoleColor();

                    Console.WriteLine();
                }
            }
            else
                Console.WriteLine(str);

        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }

    public static void ResetDefaultConsoleColor()
    {
        Console.ForegroundColor = DefaultForegroundColor;
        Console.BackgroundColor = DefaultBackgroundColor;
    }
}

