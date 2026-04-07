using System;

namespace Rinzler78.NetExtension;

/// <summary>
/// Provides thread-safe console output helpers with color support.
/// </summary>
public static class ConsoleExt
{
    /// <summary>
    /// The default foreground color captured at application startup.
    /// </summary>
    public static readonly ConsoleColor DefaultForegroundColor = Console.ForegroundColor;

    /// <summary>
    /// The default background color captured at application startup.
    /// </summary>
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

    /// <summary>
    /// Resets the console foreground and background colors to their defaults.
    /// </summary>
    public static void ResetDefaultConsoleColor()
    {
        Console.ForegroundColor = DefaultForegroundColor;
        Console.BackgroundColor = DefaultBackgroundColor;
    }
}
