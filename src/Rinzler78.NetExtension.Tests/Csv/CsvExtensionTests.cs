using Rinzler78.NetExtension.Csv;
using Rinzler78.NetExtension.Tests.TestHelpers;

namespace Rinzler78.NetExtension.Tests.Csv;

[Trait("Category", "Unit")]
public class CsvExtensionTests
{
    // ─────────────────────────────────────────────────────────────────
    // LoadCsv (synchronous)
    // ─────────────────────────────────────────────────────────────────

    [Fact]
    public void LoadCsv_WithMissingFile_ShouldReturnNull()
    {
        // Production code catches FileNotFoundException and returns null.
        var result = "missing-file.csv".LoadCsv<CsvRow>();

        result.Should().BeNull();
    }

    [Fact]
    public void LoadCsv_WithEmptyFileName_ShouldThrowArgumentException()
    {
        Action act = () => "".LoadCsv<CsvRow>();

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void LoadCsv_WithNullFileName_ShouldThrowArgumentException()
    {
        string? filePath = null;

        Action act = () => filePath!.LoadCsv<CsvRow>();

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void LoadCsv_WithDirectoryPath_ShouldReturnNull()
    {
        var result = AppContext.BaseDirectory.LoadCsv<CsvRow>();

        result.Should().BeNull();
    }

    [Fact]
    public void LoadCsv_WithMissingDirectory_ShouldReturnNull()
    {
        var missingDirectoryFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"), "missing.csv");

        var result = missingDirectoryFile.LoadCsv<CsvRow>();

        result.Should().BeNull();
    }

    [Fact]
    public void LoadCsv_WithUnreadableFile_ShouldReturnNull()
    {
        if (OperatingSystem.IsWindows())
        {
            return;
        }

        var filePath = MockHelpers.CreateTempFile("Alice;30\n");

        try
        {
            File.SetUnixFileMode(filePath, UnixFileMode.None);

            var result = filePath.LoadCsv<CsvRow>();

            result.Should().BeNull();
        }
        finally
        {
            File.SetUnixFileMode(filePath, UnixFileMode.UserRead | UnixFileMode.UserWrite);
            MockHelpers.CleanupTempFile(filePath);
        }
    }

    [Fact]
    public void LoadCsv_WithHeader_ShouldReadRecords()
    {
        // The separator is passed explicitly (';') — no auto-detection occurs.
        // CsvHelper maps columns by position when hasHeaderRecord = true.
        var filePath = MockHelpers.CreateTempFile("Name;Age\nAlice;30\nBob;40\n");

        try
        {
            var result = filePath.LoadCsv<CsvRow>(hasHeaderRecord: true)!.ToArray();

            result.Should().HaveCount(2);
            result[0].Name.Should().Be("Alice");
            result[1].Age.Should().Be(40);
        }
        finally
        {
            MockHelpers.CleanupTempFile(filePath);
        }
    }

    [Fact]
    public void LoadCsv_WithMalformedContent_ShouldReturnNull()
    {
        var filePath = MockHelpers.CreateTempFile("Name;Age\nAlice;not-a-number\n");

        try
        {
            // Production code catches CsvHelperException and returns null.
            var result = filePath.LoadCsv<CsvRow>(hasHeaderRecord: true);

            result.Should().BeNull();
        }
        finally
        {
            MockHelpers.CleanupTempFile(filePath);
        }
    }

    [Fact]
    public void LoadCsv_WithCustomSeparatorAndNoHeader_ShouldReadRecords()
    {
        var filePath = MockHelpers.CreateTempFile("Alice,30\nBob,40\n");

        try
        {
            var result = filePath.LoadCsv<CsvRow>(separator: ',', hasHeaderRecord: false)!.ToArray();

            result.Should().HaveCount(2);
            result[0].Name.Should().Be("Alice");
            result[0].Age.Should().Be(30);
            result[1].Name.Should().Be("Bob");
            result[1].Age.Should().Be(40);
        }
        finally
        {
            MockHelpers.CleanupTempFile(filePath);
        }
    }

    // ─────────────────────────────────────────────────────────────────
    // LoadCsvAsync (asynchronous wrapper)
    // ─────────────────────────────────────────────────────────────────

    [Fact]
    public async Task LoadCsvAsync_WithHeader_ShouldReadRecords()
    {
        // The default separator ';' is used — no auto-detection.
        var filePath = MockHelpers.CreateTempFile("Alice;30\n");

        try
        {
            var result = (await filePath.LoadCsvAsync<CsvRow>())!.ToArray();

            result.Should().ContainSingle();
            result[0].Name.Should().Be("Alice");
            result[0].Age.Should().Be(30);
        }
        finally
        {
            MockHelpers.CleanupTempFile(filePath);
        }
    }

    [Fact]
    public async Task LoadCsvAsync_WithNonExistentFile_ShouldReturnNull()
    {
        // LoadCsvAsync delegates to LoadCsv which catches FileNotFoundException
        // and returns null rather than propagating the exception.
        // This test documents that contract so future changes are visible.
        var result = await "this-file-does-not-exist-xyz.csv".LoadCsvAsync<CsvRow>();

        result.Should().BeNull(
            "LoadCsv catches FileNotFoundException internally and returns null");
    }

    [Fact]
    public async Task LoadCsvAsync_WithEmptyStringPath_ShouldThrowArgumentException()
    {
        // LoadCsvAsync wraps LoadCsv via Task.Run; LoadCsv throws ArgumentException
        // for null/empty file names before any file I/O is attempted.
        var act = async () => await string.Empty.LoadCsvAsync<CsvRow>();

        await act.Should().ThrowAsync<ArgumentException>(
            "LoadCsv validates the file name and throws ArgumentException for empty input");
    }

    // ─────────────────────────────────────────────────────────────────
    // Supporting types
    // ─────────────────────────────────────────────────────────────────

    private sealed class CsvRow
    {
        public string Name { get; set; } = string.Empty;

        public int Age { get; set; }
    }
}
