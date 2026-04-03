using Rinzler78.NetExtension.Objects;

namespace Rinzler78.NetExtension.Tests.Objects;

[Trait("Category", "Unit")]
public class ObjectExtensionAdditionalTests
{
    [Fact]
    public void CopyTo_WhenSetterThrows_ShouldInvokeFailureCallback()
    {
        var source = new ThrowingSource { Name = "boom" };
        var target = new ThrowingTarget();
        var failures = new List<string>();

        source.CopyTo(target, (from, to) => failures.Add($"{from.propertyInfo.Name}->{to.propertyInfo.Name}"));

        failures.Should().ContainSingle().Which.Should().Be("Name->Name");
    }

    [Fact]
    public void GetPropertyValue_WithNullArguments_ShouldThrowArgumentNullException()
    {
        Action nullSource = () => ObjectExtension.GetPropertyValue<string>(null!, "Name");
        Action nullName = () => new ThrowingSource().GetPropertyValue<string>(null!);

        nullSource.Should().Throw<ArgumentNullException>();
        nullName.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void CopyTo_WhenPropertyTypesMismatch_ShouldInvokeFailureCallback()
    {
        var source = new MismatchSource { Age = 42 };
        var target = new MismatchTarget();
        var failures = new List<string>();

        source.CopyTo(target, (from, to) => failures.Add($"{from.propertyInfo.Name}->{to.propertyInfo.Name}"));

        failures.Should().ContainSingle().Which.Should().Be("Age->Age");
    }

    [Fact]
    public void CopyTo_WhenTargetIsNull_ShouldInvokeFailureCallback()
    {
        var source = new ThrowingSource { Name = "value" };
        ThrowingTarget? target = null;
        var failures = new List<string>();

        source.CopyTo(target, (from, to) => failures.Add($"{from.propertyInfo.Name}->{to.propertyInfo.Name}"));

        failures.Should().ContainSingle().Which.Should().Be("Name->Name");
    }

    private sealed class ThrowingSource
    {
        public string Name { get; set; } = string.Empty;
    }

    private sealed class ThrowingTarget
    {
        public string Name
        {
            get => string.Empty;
            set => throw new InvalidOperationException("setter failure");
        }
    }

    private sealed class MismatchSource
    {
        public int Age { get; set; }
    }

    private sealed class MismatchTarget
    {
        public string Age { get; set; } = string.Empty;
    }
}
