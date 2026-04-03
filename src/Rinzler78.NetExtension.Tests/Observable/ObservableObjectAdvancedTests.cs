using System.ComponentModel;
using Rinzler78.NetExtension.Observable;

namespace Rinzler78.NetExtension.Tests.Observable;

[Trait("Category", "Unit")]
public class ObservableObjectAdvancedTests
{
    [Fact]
    public void Constructor_ShouldSetNickNameAndAttachDependencies()
    {
        var dependency = new TestDependency();
        var target = new TestObservableHarness(dependency);

        target.NickName.Should().Be(nameof(TestObservableHarness));
        target.Dependencies.Should().ContainSingle().Which.Should().Be(dependency);
    }

    [Fact]
    public void AttachAndDetachDependencies_ShouldHandleSmallAndLargeCollections()
    {
        var target = new TestObservableHarness();
        var small = Enumerable.Range(0, 3).Select(_ => new TestDependency()).Cast<IObservableObject>().ToArray();
        var large = Enumerable.Range(0, 12).Select(_ => new TestDependency()).Cast<IObservableObject>().ToArray();

        target.Attach(small);
        target.Dependencies.Should().HaveCount(3);

        target.Detach();
        target.Dependencies.Should().BeEmpty();

        target.Attach(large);
        target.Dependencies.Should().HaveCount(12);

        target.Detach(large.Take(2).ToArray());
        target.Dependencies.Should().HaveCount(10);
    }

    [Fact]
    public void DependencyPropertyChanged_ShouldFlowToOverride()
    {
        var dependency = new TestDependency();
        var target = new TestObservableHarness(dependency);

        dependency.Name = "updated";

        target.DependencyNotifications.Should().ContainSingle().Which.Should().Be(nameof(TestDependency.Name));
    }

    [Fact]
    public void SetProperty_WithCustomValidity_ShouldUseCallback()
    {
        var target = new TestObservableHarness();

        var changed = target.SetValueWithCustomValidity(1, pair => pair.OldValue == pair.NewValue);
        var ignored = target.SetValueWithCustomValidity(1, pair => pair.OldValue == pair.NewValue);

        changed.Should().BeTrue();
        ignored.Should().BeFalse();
    }

    [Fact]
    public void Dispose_ShouldDetachDependencies()
    {
        var dependency = new TestDependency();
        var target = new TestObservableHarness(dependency);

        ((IDisposable)target).Dispose();

        target.Dependencies.Should().BeEmpty();
    }

    private sealed class TestObservableHarness : ObservableObject
    {
        private int _value;

        public TestObservableHarness(params IObservableObject[] dependencies)
            : base(dependencies)
        {
        }

        public List<string> DependencyNotifications { get; } = new();

        public void Attach(params IObservableObject[] dependencies) => AttachDependencies(dependencies);

        public void Detach(params IObservableObject[] dependencies) => DetachDependencies(dependencies);

        public bool SetValueWithCustomValidity(int value, Func<(int OldValue, int NewValue), bool> predicate)
            => SetProperty(ref _value, value, checkValidity: predicate);

        protected override void OnDependenciesPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName is not null)
                DependencyNotifications.Add(e.PropertyName);
        }
    }

    private sealed class TestDependency : ObservableObject
    {
        private string _name = string.Empty;

        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }
    }
}
