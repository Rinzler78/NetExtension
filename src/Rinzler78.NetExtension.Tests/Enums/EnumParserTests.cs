using FluentAssertions;
using Rinzler78.NetExtension.Enums;

namespace Rinzler78.NetExtension.Tests.Enums;

[Trait("Category", "Unit")]
[Collection("Console serial")]
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
    public void Parser_WithLongBackedEnum_ShouldParseNumericValue()
    {
        var parser = new EnumParser<LongBackedParserEnum>();
        parser.Parse("2147483648").Should().Be(LongBackedParserEnum.VeryLarge);
    }

    [Fact]
    public void Parser_WithNegativeSignedEnum_ShouldParseNumericValue()
    {
        var parser = new EnumParser<SignedParserEnum>();
        parser.Parse("-1").Should().Be(SignedParserEnum.NegativeOne);
    }

    [Fact]
    public void Parser_WithUnknownValue_ShouldReturnDefault()
    {
        var parser = new EnumParser<ParserEnum>();
        parser.Parse("missing").Should().Be(default(ParserEnum));
    }

    [Fact]
    public void Parse_Null_ShouldReturnDefault()
    {
        var parser = new EnumParser<ParserEnum>();
        parser.Parse(null!).Should().Be(default(ParserEnum));
    }

    [Fact]
    public void Parse_InvalidValue_ShouldReturnDefault()
    {
        var parser = new EnumParser<ParserEnum>();
        parser.Parse("invalid_value").Should().Be(default(ParserEnum));
    }

    [Fact]
    public void Parse_SameValueRepeatedCalls_ShouldReturnConsistentResult()
    {
        var parser = new EnumParser<ParserEnum>();
        var first = parser.Parse("Value_One");
        var second = parser.Parse("Value_One");
        first.Should().Be(ParserEnum.Value_One);
        first.Should().Be(second);
    }

    [Fact]
    public void Parse_InvalidValue_ShouldNotWriteToConsole()
    {
        var parser = new EnumParser<ParserEnum>();
        using var writer = new StringWriter();
        var original = global::System.Console.Out;
        try
        {
            global::System.Console.SetOut(writer);
            parser.Parse("missing");
            writer.ToString().Should().BeEmpty();
        }
        finally
        {
            global::System.Console.SetOut(original);
        }
    }

    private enum ParserEnum { Value_One = 1, ValueTwo = 2 }
    private enum LongBackedParserEnum : long { VeryLarge = 2147483648L }
    private enum SignedParserEnum : int { NegativeOne = -1, Zero = 0 }
}
