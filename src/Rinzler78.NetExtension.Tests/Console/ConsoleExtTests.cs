using System.Text;
using Rinzler78.NetExtension;

namespace Rinzler78.NetExtension.Tests.Console;

[Collection("Console serial")]
[Trait("Category", "Unit")]
public class ConsoleExtTests
{
    [Fact]
    public void ConsoleWriteLine_WithCurrentColors_ShouldWriteLine()
    {
        using var capture = new ConsoleCaptureTextWriter();
        var originalOut = System.Console.Out;

        try
        {
            System.Console.SetOut(capture);

            "plain output".ConsoleWriteLine();

            capture.ToString().Should().Contain("plain output");
        }
        finally
        {
            System.Console.SetOut(originalOut);
        }
    }

    [Fact]
    public void ConsoleWriteLine_WithDifferentColors_ShouldResetDefaults()
    {
        using var capture = new ConsoleCaptureTextWriter();
        var originalOut = System.Console.Out;
        var originalForeground = System.Console.ForegroundColor;
        var originalBackground = System.Console.BackgroundColor;

        try
        {
            System.Console.SetOut(capture);
            System.Console.ForegroundColor = ConsoleColor.Yellow;
            System.Console.BackgroundColor = ConsoleColor.DarkBlue;

            "colored output".ConsoleWriteLine(ConsoleColor.Black, ConsoleColor.White);

            capture.ToString().Should().Contain("colored output");
            System.Console.ForegroundColor.Should().Be(ConsoleExt.DefaultForegroundColor);
            System.Console.BackgroundColor.Should().Be(ConsoleExt.DefaultBackgroundColor);
        }
        finally
        {
            System.Console.ForegroundColor = originalForeground;
            System.Console.BackgroundColor = originalBackground;
            System.Console.SetOut(originalOut);
        }
    }

    [Fact]
    public void ConsoleWriteLine_WithInvalidConsoleColor_ShouldWriteArgumentError()
    {
        using var capture = new ConsoleCaptureTextWriter();
        var originalOut = System.Console.Out;

        try
        {
            System.Console.SetOut(capture);

            "bad colors".ConsoleWriteLine((ConsoleColor)int.MaxValue, ConsoleColor.White);

            capture.ToString().Should().Contain("Invalid console color argument:");
        }
        finally
        {
            System.Console.SetOut(originalOut);
            ConsoleExt.ResetDefaultConsoleColor();
        }
    }

    [Fact]
    public void ConsoleWriteLine_WhenConsoleWriterThrowsIOException_ShouldWriteIoError()
    {
        using var capture = new ThrowingConsoleWriter(new IOException("io boom"));
        var originalOut = System.Console.Out;

        try
        {
            System.Console.SetOut(capture);

            "io failure".ConsoleWriteLine();

            capture.ToString().Should().Contain("Console I/O error: io boom");
        }
        finally
        {
            System.Console.SetOut(originalOut);
        }
    }

    [Fact]
    public void ConsoleWriteLine_WhenConsoleWriterThrowsUnexpectedException_ShouldWriteUnexpectedError()
    {
        using var capture = new ThrowingConsoleWriter(new InvalidOperationException("unexpected boom"));
        var originalOut = System.Console.Out;

        try
        {
            System.Console.SetOut(capture);

            "unexpected failure".ConsoleWriteLine();

            capture.ToString().Should().Contain("Unexpected error in console output: unexpected boom");
        }
        finally
        {
            System.Console.SetOut(originalOut);
        }
    }

    // ─────────────────────────────────────────────────────────────────
    // ResetDefaultConsoleColor
    // ─────────────────────────────────────────────────────────────────

    [Fact]
    public void ResetDefaultConsoleColor_ShouldRestoreStartupColors()
    {
        var originalForeground = System.Console.ForegroundColor;
        var originalBackground = System.Console.BackgroundColor;

        try
        {
            // Change colors
            System.Console.ForegroundColor = ConsoleColor.Magenta;
            System.Console.BackgroundColor = ConsoleColor.DarkCyan;

            // Reset
            ConsoleExt.ResetDefaultConsoleColor();

            // Should be back to startup defaults
            System.Console.ForegroundColor.Should().Be(ConsoleExt.DefaultForegroundColor);
            System.Console.BackgroundColor.Should().Be(ConsoleExt.DefaultBackgroundColor);
        }
        finally
        {
            System.Console.ForegroundColor = originalForeground;
            System.Console.BackgroundColor = originalBackground;
        }
    }

    [Fact]
    public void ConsoleWriteLine_WithNullForegroundAndValidBackground_ShouldUseDefaultForeground()
    {
        using var capture = new ConsoleCaptureTextWriter();
        var originalOut = System.Console.Out;

        try
        {
            System.Console.SetOut(capture);

            "bg only".ConsoleWriteLine(ConsoleColor.DarkBlue, null);

            capture.ToString().Should().Contain("bg only");
        }
        finally
        {
            System.Console.SetOut(originalOut);
            ConsoleExt.ResetDefaultConsoleColor();
        }
    }

    [Fact]
    public void ConsoleWriteLine_WithNullBothColors_ShouldUseDefaults()
    {
        using var capture = new ConsoleCaptureTextWriter();
        var originalOut = System.Console.Out;

        try
        {
            System.Console.SetOut(capture);

            "default colors".ConsoleWriteLine(null, null);

            capture.ToString().Should().Contain("default colors");
        }
        finally
        {
            System.Console.SetOut(originalOut);
        }
    }

    private sealed class ConsoleCaptureTextWriter : StringWriter
    {
        public override Encoding Encoding => Encoding.UTF8;
    }

    private sealed class ThrowingConsoleWriter : StringWriter
    {
        private readonly Exception _exception;
        private bool _shouldThrow = true;

        public ThrowingConsoleWriter(Exception exception)
        {
            _exception = exception;
        }

        public override Encoding Encoding => Encoding.UTF8;

        public override void WriteLine(string? value)
        {
            if (_shouldThrow)
            {
                _shouldThrow = false;
                throw _exception;
            }

            base.WriteLine(value);
        }
    }
}
