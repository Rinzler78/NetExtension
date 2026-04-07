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

    // ── Gap 1: SetProperty with IObservableObject property ─────────────────
    [Fact]
    public void SetProperty_WithIObservableObjectValue_AutoAttachesAndDetachesDependencies()
    {
        // Arrange
        var harness = new ObservableObjectWithChildProp();
        var depA = new TestDependency();
        var depB = new TestDependency();

        // Act – assign depA → auto-attached
        harness.Child = depA;
        harness.Dependencies.Should().ContainSingle().Which.Should().Be(depA);

        // Act – replace with depB → depA detached, depB attached
        harness.Child = depB;
        harness.Dependencies.Should().ContainSingle().Which.Should().Be(depB);

        // Act – set to null → depB detached, Dependencies empty
        harness.Child = null;
        harness.Dependencies.Should().BeEmpty();
    }

    // ── Gap 2: propertyChanged callback ────────────────────────────────────
    [Fact]
    public void SetProperty_WithPropertyChangedCallback_InvokesCallbackWithOldAndNewValues()
    {
        // Arrange
        var harness = new TestObservableHarnessWithCallback();
        (string? Old, string? New)? captured = null;

        // Act
        harness.SetName("hello", pair => captured = (pair.OldValue, pair.NewValue));

        // Assert
        captured.Should().NotBeNull();
        captured!.Value.Old.Should().BeNull();
        captured!.Value.New.Should().Be("hello");
    }

    // ── Gap 3: Dispose() × 2 idempotent ────────────────────────────────────
    [Fact]
    public void Dispose_CalledTwice_DoesNotThrow()
    {
        var harness = new TestObservableHarness();
        var act = () =>
        {
            ((IDisposable)harness).Dispose();
            ((IDisposable)harness).Dispose();
        };
        act.Should().NotThrow();
    }

    // ── Gap 4: OnDependenciesPropertyChanged base no-op ─────────────────────
    [Fact]
    public void OnDependenciesPropertyChanged_BaseImplementation_DoesNotThrow()
    {
        // TestObservableObjectNoOverride does NOT override OnDependenciesPropertyChanged
        var noOverride = new TestObservableObjectNoOverride();
        var dep = new TestObservableObjectNoOverride { Name = "dep" };
        noOverride.AttachDependenciesPublic(dep);

        // Trigger PropertyChanged on the dependency → base no-op must not throw
        var act = () => { dep.Name = "changed"; };
        act.Should().NotThrow();
    }

    // ── Gap 6: Dispose(bool disposing=false) ────────────────────────────────
    [Fact]
    public void Dispose_WithDisposingFalse_DoesNotClearPropertyChanged()
    {
        // Arrange – Dispose(false) must NOT null out PropertyChanged
        var harness = new TestObservableHarnessWithExposedDispose();
        bool eventRaised = false;
        harness.PropertyChanged += (_, _) => eventRaised = true;

        // Act – call protected Dispose(false)
        harness.ExposeDispose(disposing: false);

        // Assert – handler is still intact
        harness.SetName("test");
        eventRaised.Should().BeTrue();
    }

    // ── Existing helpers ───────────────────────────────────────────────────

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

    // ── Helpers for new tests ──────────────────────────────────────────────

    /// <summary>Exposes a child IObservableObject property to test auto attach/detach.</summary>
    private sealed class ObservableObjectWithChildProp : ObservableObject
    {
        private IObservableObject? _child;

        public IObservableObject? Child
        {
            get => _child;
            set => SetProperty(ref _child, value);
        }
    }

    /// <summary>Exposes SetProperty with the propertyChanged callback.</summary>
    private sealed class TestObservableHarnessWithCallback : ObservableObject
    {
        private string? _name;

        public bool SetName(string? value, Action<(string? OldValue, string? NewValue)> callback)
            => SetProperty(ref _name, value, propertyChanged: callback);
    }

    /// <summary>Does NOT override OnDependenciesPropertyChanged – tests base no-op.</summary>
    private sealed class TestObservableObjectNoOverride : ObservableObject
    {
        private string _name = string.Empty;

        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        public void AttachDependenciesPublic(params IObservableObject[] deps)
            => AttachDependencies(deps);
    }

    /// <summary>Exposes protected Dispose(bool) for testing the disposing=false branch.</summary>
    private sealed class TestObservableHarnessWithExposedDispose : ObservableObject
    {
        private string _name = string.Empty;

        public string Name { get => _name; set => SetProperty(ref _name, value); }
        public void SetName(string v) => Name = v;
        public void ExposeDispose(bool disposing) => Dispose(disposing);
    }
}
