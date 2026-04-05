using System;
using System.Threading;
using System.Threading.Tasks;
using Rinzler78.NetExtension.Observable;

namespace Rinzler78.NetExtension.Objects;

/// <summary>
/// Abstract base for lazily-initialized, asynchronously-fetched observable properties.
/// Uses a <see cref="SemaphoreSlim"/> to provide async-safe mutual exclusion,
/// ensuring <c>_initialized</c> is read and written atomically and that
/// <see cref="InnerUpdate"/> is called at most once on concurrent first access.
/// </summary>
public abstract class UpdatableProperty : ObservableObject
{
    // SemaphoreSlim(1,1) replaces object _locker to allow await inside the critical section.
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    private Task<object?> _getTask = Task.FromResult<object?>(null);
    private object? _property;
    private Task _updateTask = Task.CompletedTask;
    private bool _initialized;

    /// <summary>Gets or sets the cached property value.</summary>
    public object? Property
    {
        get => _property;
        protected set => SetProperty(ref _property, value);
    }

    /// <summary>
    /// Returns the cached value, fetching it first if not yet initialized or if
    /// <paramref name="force"/> is <see langword="true"/>.
    /// Concurrent calls are deduplicated: only one fetch runs at a time.
    /// </summary>
    /// <param name="force">When <see langword="true"/>, bypasses the cache and re-fetches.</param>
    public Task<object?> Get(bool force = false)
    {
        // Task-deduplication check outside semaphore for the common (already-running) path.
        // The semaphore protects _initialized and _getTask assignment.
        Task<object?> snapshot;
        _semaphore.Wait();
        try
        {
            if (_getTask.IsCompleted)
                _getTask = GetCore(force);
            snapshot = _getTask;
        }
        finally
        {
            _semaphore.Release();
        }
        return snapshot;
    }

    /// <summary>
    /// Triggers a refresh of the cached value. Concurrent calls are deduplicated.
    /// </summary>
    public Task Update()
    {
        _semaphore.Wait();
        try
        {
            if (_updateTask.IsCompleted)
                _updateTask = UpdateCore();
            return _updateTask;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private async Task<object?> GetCore(bool force)
    {
        // Double-check _initialized inside the async body.
        // The semaphore was already acquired in Get(); we release it before awaiting
        // to avoid holding it across the async call (async-over-sync anti-pattern).
        // Because _getTask deduplication is handled in Get(), this body runs only once
        // per completed cycle.
        if (!_initialized || force)
        {
            await UpdateCore().ConfigureAwait(false);
        }
        return Property;
    }

    private async Task UpdateCore()
    {
        await InnerUpdate().ConfigureAwait(false);

        // Acquire semaphore to write _initialized safely.
        await _semaphore.WaitAsync().ConfigureAwait(false);
        try
        {
            _initialized = true;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    /// <summary>Performs the actual fetch and updates <see cref="Property"/>.</summary>
    protected abstract Task InnerUpdate();
}

/// <summary>
/// Strongly-typed lazily-initialized observable property backed by an async factory function.
/// </summary>
/// <typeparam name="PropertyType">The type of the cached value.</typeparam>
public sealed class UpdatableProperty<PropertyType> : UpdatableProperty
{
    /// <summary>
    /// Initializes a new instance with the specified async factory.
    /// </summary>
    /// <param name="updatableFunction">The async function used to fetch the value. Must not be null.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="updatableFunction"/> is null.</exception>
    public UpdatableProperty(Func<Task<PropertyType>> updatableFunction)
    {
        UpdatableFunction = updatableFunction
            ?? throw new ArgumentNullException(nameof(updatableFunction),
                $"{nameof(updatableFunction)} must be set");
    }

    /// <summary>Gets the strongly-typed cached value.</summary>
    public new PropertyType? Property => (PropertyType?)base.Property;

    private Func<Task<PropertyType>> UpdatableFunction { get; }

    /// <summary>
    /// Returns the cached value as <typeparamref name="PropertyType"/>,
    /// fetching it first if not yet initialized or if <paramref name="force"/> is true.
    /// </summary>
    public new async Task<PropertyType?> Get(bool force = false)
        => (PropertyType?)await base.Get(force).ConfigureAwait(false);

    /// <inheritdoc/>
    protected override async Task InnerUpdate()
    {
        base.Property = await UpdatableFunction.Invoke().ConfigureAwait(false);
    }
}
