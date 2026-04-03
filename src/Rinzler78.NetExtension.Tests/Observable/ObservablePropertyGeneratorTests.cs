using System.Collections.Generic;
using System.Reflection;
using Rinzler78.NetExtension.Observable;
using Xunit;

namespace Rinzler78.NetExtension.Tests.Observable;

[Trait("Category", "Unit")]
public class ObservablePropertyGeneratorTests
{
    // ── Naming: verified through generated output ───────────────────────────

    [Fact]
    public void Generator_UnderscoreField_GeneratesPascalCaseProperty()
    {
        // _name → Name
        var vm = new NamingUnderscoreViewModel();
        vm.Name = "Alice";
        vm.Name.Should().Be("Alice");
    }

    [Fact]
    public void Generator_MUnderscoreField_GeneratesPascalCaseProperty()
    {
        // m_value → Value
        var vm = new NamingMPrefixViewModel();
        vm.Value = 42;
        vm.Value.Should().Be(42);
    }

    [Fact]
    public void Generator_CamelCaseField_GeneratesPascalCaseProperty()
    {
        // myTitle → MyTitle
        var vm = new NamingCamelViewModel();
        vm.MyTitle = "Hello";
        vm.MyTitle.Should().Be("Hello");
    }

    // ── Attribute contract ──────────────────────────────────────────────────

    [Fact]
    public void ObservablePropertyAttribute_DefaultCtor_HasNullPropertyName()
        => new ObservablePropertyAttribute().PropertyName.Should().BeNull();

    [Fact]
    public void ObservablePropertyAttribute_ExplicitName_ExposesIt()
        => new ObservablePropertyAttribute("MyProp").PropertyName.Should().Be("MyProp");

    [Fact]
    public void ObservablePropertyAttribute_TargetsFields_NotAllowMultiple()
    {
        var usage = (AttributeUsageAttribute)typeof(ObservablePropertyAttribute)
            .GetCustomAttributes(typeof(AttributeUsageAttribute), false)[0];
        usage.ValidOn.Should().HaveFlag(AttributeTargets.Field);
        usage.AllowMultiple.Should().BeFalse();
    }

    // ── Integration: generated property semantics ───────────────────────────

    [Fact]
    public void GeneratedProperty_Get_ReturnsDefaultValue()
    {
        var vm = new SampleViewModel();
        vm.Name.Should().Be(string.Empty);
        vm.Age.Should().Be(0);
    }

    [Fact]
    public void GeneratedProperty_Set_UpdatesValue()
    {
        var vm = new SampleViewModel();
        vm.Name = "Alice";
        vm.Age = 30;
        vm.Name.Should().Be("Alice");
        vm.Age.Should().Be(30);
    }

    [Fact]
    public void GeneratedProperty_Set_RaisesPropertyChanged()
    {
        var vm = new SampleViewModel();
        var raised = new List<string?>();
        vm.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

        vm.Name = "Alice";
        vm.Age = 30;

        raised.Should().Contain("Name").And.Contain("Age");
    }

    [Fact]
    public void GeneratedProperty_SetSameValue_DoesNotRaisePropertyChanged()
    {
        var vm = new SampleViewModel();
        vm.Name = "Alice";
        var raised = new List<string?>();
        vm.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

        vm.Name = "Alice"; // same value — no event expected
        raised.Should().BeEmpty();
    }

    [Fact]
    public void GeneratedProperty_ExplicitName_IsUsed()
    {
        var vm = new SampleWithExplicitName();
        vm.FullName = "Bob";
        vm.FullName.Should().Be("Bob");
    }

    [Fact]
    public void GeneratedProperty_ExistsAsPublicProperty()
    {
        typeof(SampleViewModel)
            .GetProperty("Name", BindingFlags.Public | BindingFlags.Instance)
            .Should().NotBeNull("source generator must emit a public Name property");

        typeof(SampleViewModel)
            .GetProperty("Age", BindingFlags.Public | BindingFlags.Instance)
            .Should().NotBeNull("source generator must emit a public Age property");
    }
}

// ── Test fixtures ─────────────────────────────────────────────────────────────

public partial class SampleViewModel : ObservableObject
{
    [ObservableProperty] private string _name = string.Empty;
    [ObservableProperty] private int _age;
}

public partial class SampleWithExplicitName : ObservableObject
{
    [ObservableProperty("FullName")]
    private string _fullName = string.Empty;
}

public partial class NamingUnderscoreViewModel : ObservableObject
{
    [ObservableProperty] private string _name = string.Empty;
}

public partial class NamingMPrefixViewModel : ObservableObject
{
    [ObservableProperty] private int m_value;
}

public partial class NamingCamelViewModel : ObservableObject
{
    [ObservableProperty] private string myTitle = string.Empty;
}
