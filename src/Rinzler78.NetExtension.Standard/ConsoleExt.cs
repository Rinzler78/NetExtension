using System;

namespace Rinzler78.NetExtension;

public static class ConsoleExt
{
    public static readonly ConsoleColor DefaultForegroundColor = Console.ForegroundColor;
    public static readonly ConsoleColor DefaultBackgroundColor = Console.BackgroundColor;

    private static readonly object Locker = new();

    /// <summary>
    /// Writes a line to the console with optional background and foreground colors.
    /// </summary>
    /// <param name="str">The string to write.</param>
    /// <param name="backgroundColor">The background color (optional).</param>
    /// <param name="foregroundColor">The foreground color (optional).</param>
    /// <exception cref="ArgumentException">Thrown when invalid console color values are provided.</exception>
    /// <exception cref="System.IO.IOException">Thrown when console I/O operations fail.</exception>
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
            {
                Console.WriteLine(str);
            }
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine($"Invalid console color argument: {ex.Message}");
        }
        catch (System.IO.IOException ex)
        {
            Console.WriteLine($"Console I/O error: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Unexpected error in console output: {ex.Message}");
        }
    }

    public static void ResetDefaultConsoleColor()
    {
        Console.ForegroundColor = DefaultForegroundColor;
        Console.BackgroundColor = DefaultBackgroundColor;
    }
}
