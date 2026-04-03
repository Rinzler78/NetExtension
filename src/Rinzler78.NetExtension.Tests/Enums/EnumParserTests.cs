using FluentAssertions;
using Rinzler78.NetExtension.Enums;

namespace Rinzler78.NetExtension.Tests.Enums;

[Trait("Category", "Unit")]
public class EnumParserTests
{
    [Fact]
    public void Parser_ShouldExposeAliasesAndParseValues()
    {
        var parser = new EnumParser<ParserEnum>();

        parser.EnumValuesDictionary.Should().ContainKey("1");
        parser.EnumValuesDictionary.Should().ContainKey("value.one");
        parser.Parse("Value_One").Should().Be(ParserEnum.Value_One);
        parser.Parse("2").Should().Be(ParserEnum.ValueTwo);
    }

    [Fact]
    public void Parser_WithUnknownValue_ShouldReturnDefault()
    {
        var parser = new EnumParser<ParserEnum>();

        parser.Parse("missing").Should().Be(default(ParserEnum));
    }

    /// <summary>
    /// EnumParser.Parse catches the ArgumentNullException thrown by the underlying
    /// ReadOnlyDictionary when a null key is looked up and returns the nullable default.
    /// FA resolves <c>Enumtype?</c> to <c>EnumAssertions&lt;T&gt;</c>, so we assert via
    /// <c>Be(default(ParserEnum))</c>, consistent with <see cref="Parser_WithUnknownValue_ShouldReturnDefault"/>.
    /// </summary>
    [Fact]
    public void Parse_Null_ShouldReturnDefault()
    {
        var parser = new EnumParser<ParserEnum>();

        // Dictionary throws ArgumentNullException for null keys; the catch block in
        // Parse swallows it and returns default, equivalent to (ParserEnum)0.
        parser.Parse(null!).Should().Be(default(ParserEnum),
            "Parse catches ArgumentNullException from the dictionary and returns default");
    }

    /// <summary>
    /// An unrecognised string (including values with underscores that normalise to a
    /// dot-separated key not present in the dictionary) yields a KeyNotFoundException
    /// that is caught and converted to the default value.
    /// </summary>
    [Fact]
    public void Parse_InvalidValue_ShouldReturnDefault()
    {
        var parser = new EnumParser<ParserEnum>();

        // "invalid_value" is normalised to "invalid.value" internally; neither key
        // exists in the dictionary → KeyNotFoundException caught → default returned.
        parser.Parse("invalid_value").Should().Be(default(ParserEnum),
            "Parse catches KeyNotFoundException for unknown keys and returns default");
    }

    /// <summary>
    /// Calling Parse with the same valid value multiple times must return the same
    /// result each time, verifying that the underlying ReadOnlyDictionary look-up
    /// is idempotent (effectively a caching / no-side-effect guarantee).
    /// </summary>
    [Fact]
    public void Parse_SameValueRepeatedCalls_ShouldReturnConsistentResult()
    {
        var parser = new EnumParser<ParserEnum>();

        var first = parser.Parse("Value_One");
        var second = parser.Parse("Value_One");
        var third = parser.Parse("Value_One");

        first.Should().Be(ParserEnum.Value_One);
        second.Should().Be(ParserEnum.Value_One);
        third.Should().Be(ParserEnum.Value_One);
        // All calls must agree — no mutation between invocations.
        first.Should().Be(second);
        second.Should().Be(third);
    }

    private enum ParserEnum
    {
        Value_One = 1,
        ValueTwo = 2
    }
}
