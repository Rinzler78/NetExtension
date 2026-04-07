using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using FluentAssertions;
using Rinzler78.NetExtension.Enums;
using Xunit;

namespace Rinzler78.NetExtension.Tests.Enums;

/// <summary>
/// Unit tests for <see cref="GlobalEnumParser"/>.
/// Covers normal parsing, edge-case inputs (invalid / null), dictionary idempotency, and
/// thread-safety of the static lock that guards <c>EnumParsersDictionary</c>.
/// </summary>
[Trait("Category", "Unit")]
public class GlobalEnumParserTests
{
    // -------------------------------------------------------------------------
    // Happy-path parsing
    // -------------------------------------------------------------------------

    [Fact]
    public void Parse_KnownEnumName_ReturnsCorrectValue()
    {
        // The underlying EnumParser stores keys in lower-case via OrdinalIgnoreCase;
        // "Monday" therefore matches the "monday" key.
        var result = GlobalEnumParser.Parse<DayOfWeek>("Monday");

        result.Should().Be(DayOfWeek.Monday);
    }

    [Theory]
    [InlineData("Sunday", DayOfWeek.Sunday)]
    [InlineData("monday", DayOfWeek.Monday)]
    [InlineData("TUESDAY", DayOfWeek.Tuesday)]
    [InlineData("Wednesday", DayOfWeek.Wednesday)]
    [InlineData("thursday", DayOfWeek.Thursday)]
    [InlineData("Friday", DayOfWeek.Friday)]
    [InlineData("SATURDAY", DayOfWeek.Saturday)]
    public void Parse_AllDaysOfWeek_AreParsedCaseInsensitively(string input, DayOfWeek expected)
    {
        // EnumParser's dictionary is built with OrdinalIgnoreCase StringComparer,
        // so case does not affect the lookup result.
        var result = GlobalEnumParser.Parse<DayOfWeek>(input);

        result.Should().Be(expected);
    }

    [Fact]
    public void Parse_NumericStringMatchingMemberValue_ReturnsCorrectEnumValue()
    {
        // EnumParser adds the numeric value as a key as well (e.g. "1" → Monday).
        var result = GlobalEnumParser.Parse<DayOfWeek>("1");

        result.Should().Be(DayOfWeek.Monday);
    }

    // -------------------------------------------------------------------------
    // Invalid input — documented behavior: default(EnumType?) == null
    // -------------------------------------------------------------------------

    /// <summary>
    /// <see cref="GlobalEnumParser.Parse{EnumType}"/> delegates to
    /// <see cref="EnumParser{Enumtype}.Parse"/> which catches any exception from the
    /// dictionary look-up and returns <c>default</c>.  For a nullable enum the
    /// default is <c>null</c>.
    /// </summary>
    [Fact]
    public void Parse_InvalidString_ReturnsDefault()
    {
        // "invalid" is not a member name or numeric value of DayOfWeek.
        // No exception is propagated; the method returns null.
        var result = GlobalEnumParser.Parse<DayOfWeek>("invalid");

        result.Should().Be(default(DayOfWeek),
            "Parse catches the KeyNotFoundException and returns default(DayOfWeek) — DayOfWeek.Sunday (0)");
    }

    /// <summary>
    /// A <c>null</c> key causes the underlying ReadOnlyDictionary to throw
    /// <see cref="ArgumentNullException"/>.  The catch block in
    /// <see cref="EnumParser{Enumtype}.Parse"/> swallows this and returns <c>default</c>.
    /// </summary>
    [Fact]
    public void Parse_NullString_ReturnsDefault()
    {
        // No exception should escape; the null is handled internally.
        var result = GlobalEnumParser.Parse<DayOfWeek>(null!);

        result.Should().Be(default(DayOfWeek),
            "Parse catches ArgumentNullException from the dictionary and returns default(DayOfWeek) — DayOfWeek.Sunday (0)");
    }

    // -------------------------------------------------------------------------
    // Dictionary idempotency: two consecutive calls yield the same parser / result
    // -------------------------------------------------------------------------

    [Fact]
    public void Parse_SameEnumType_ConsecutiveCalls_ReturnConsistentResult()
    {
        // The first call populates the dictionary; the second re-uses the stored parser.
        // Both calls must return identical results.
        var first = GlobalEnumParser.Parse<DayOfWeek>("Wednesday");
        var second = GlobalEnumParser.Parse<DayOfWeek>("Wednesday");

        first.Should().Be(DayOfWeek.Wednesday);
        second.Should().Be(DayOfWeek.Wednesday);
        first.Should().Be(second,
            "caching must be idempotent — same input always yields same output");
    }

    [Fact]
    public void Parse_SameEnumType_ManyConsecutiveCalls_AlwaysReturnSameResult()
    {
        var results = Enumerable
            .Range(0, 20)
            .Select(_ => GlobalEnumParser.Parse<DayOfWeek>("Friday"))
            .ToList();

        results.Should().AllSatisfy(r => r.Should().Be(DayOfWeek.Friday));
    }

    // -------------------------------------------------------------------------
    // Thread safety: concurrent first-use for distinct and identical enum types
    // -------------------------------------------------------------------------

    /// <summary>
    /// Simultaneously resolves parsers for two different enum types from many threads.
    /// The static <c>lock(EnumParsersDictionary)</c> must serialize dictionary writes
    /// without deadlock or data corruption.
    /// </summary>
    [Fact]
    public void Parse_ConcurrentFirstUse_DifferentEnumTypes_DoesNotThrowAndReturnsCorrectValues()
    {
        const int threadCount = 16;
        var exceptions = new List<Exception>();
        var dayResults = new DayOfWeek?[threadCount];
        var colorResults = new ConsoleColor?[threadCount];
        var barrier = new Barrier(threadCount);

        var threads = Enumerable.Range(0, threadCount).Select(i => new Thread(() =>
        {
            try
            {
                barrier.SignalAndWait(); // maximise contention at first-use
                dayResults[i] = GlobalEnumParser.Parse<DayOfWeek>("Thursday");
                colorResults[i] = GlobalEnumParser.Parse<ConsoleColor>("Red");
            }
            catch (Exception ex)
            {
                lock (exceptions)
                    exceptions.Add(ex);
            }
        })).ToList();

        threads.ForEach(t => t.Start());
        threads.ForEach(t => t.Join(TimeSpan.FromSeconds(10)));

        exceptions.Should().BeEmpty(
            "the static lock must prevent races in EnumParsersDictionary");
        dayResults.Should().AllSatisfy(r => r.Should().Be(DayOfWeek.Thursday));
        colorResults.Should().AllSatisfy(r => r.Should().Be(ConsoleColor.Red));
    }

    /// <summary>
    /// Simultaneously parses the same enum type from many threads.
    /// After the first thread inserts the parser, subsequent threads read from storage.
    /// The lock must also protect this read path correctly.
    /// </summary>
    [Fact]
    public void Parse_ConcurrentCallsSameEnumType_DoesNotThrowAndReturnsCorrectValues()
    {
        const int threadCount = 32;
        var exceptions = new List<Exception>();
        var results = new DayOfWeek?[threadCount];
        var barrier = new Barrier(threadCount);

        var threads = Enumerable.Range(0, threadCount).Select(i => new Thread(() =>
        {
            try
            {
                barrier.SignalAndWait();
                results[i] = GlobalEnumParser.Parse<DayOfWeek>("Saturday");
            }
            catch (Exception ex)
            {
                lock (exceptions)
                    exceptions.Add(ex);
            }
        })).ToList();

        threads.ForEach(t => t.Start());
        threads.ForEach(t => t.Join(TimeSpan.FromSeconds(10)));

        exceptions.Should().BeEmpty(
            "concurrent lookups on the same cached parser must be safe");
        results.Should().AllSatisfy(r => r.Should().Be(DayOfWeek.Saturday));
    }
}
