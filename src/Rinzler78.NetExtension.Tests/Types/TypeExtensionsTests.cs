using System.Reflection;
using Rinzler78.NetExtension.Types;

namespace Rinzler78.NetExtension.Tests.Types;

[Trait("Category", "Unit")]
public class TypeExtensionsTests
{
    [Theory]
    [InlineData(typeof(int), true)]
    [InlineData(typeof(string), true)]
    [InlineData(typeof(Guid), true)]
    [InlineData(typeof(DateTimeOffset), true)]
    [InlineData(typeof(TypeExtensionsTests), false)]
    public void IsSimpleType_ShouldMatchExpectedClassification(Type type, bool expected)
    {
        type.IsSimpleType().Should().Be(expected);
    }

    [Fact]
    public void GetUnderlyingType_ShouldSupportEventFieldMethodAndProperty()
    {
        typeof(TestMemberHost).GetEvent(nameof(TestMemberHost.Changed))!.GetUnderlyingType().Should().Be(typeof(EventHandler));
        typeof(TestMemberHost).GetField(nameof(TestMemberHost.Counter))!.GetUnderlyingType().Should().Be(typeof(int));
        typeof(TestMemberHost).GetMethod(nameof(TestMemberHost.Build))!.GetUnderlyingType().Should().Be(typeof(string));
        typeof(TestMemberHost).GetProperty(nameof(TestMemberHost.Name))!.GetUnderlyingType().Should().Be(typeof(string));
    }

    [Fact]
    public void GetUnderlyingType_WithUnsupportedMember_ShouldThrowArgumentException()
    {
        var member = typeof(TypeExtensionsTests).GetConstructor(Type.EmptyTypes)!;

        Action act = () => member.GetUnderlyingType();

        act.Should().Throw<ArgumentException>();
    }

    private sealed class TestMemberHost
    {
        public int Counter = 1;

        public string Name { get; set; } = string.Empty;

        public event EventHandler? Changed;

        public string Build() => Name;
    }
}
