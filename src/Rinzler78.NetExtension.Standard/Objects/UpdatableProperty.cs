using Rinzler78.NetExtension.Observable;
using System;
using System.Threading.Tasks;

namespace Rinzler78.NetExtension.Objects;

public abstract class UpdatableProperty : ObservableObject
{
    private readonly object _locker = new object();

    private Task<object?> _getTask = Task.FromResult<object?>(null);

    private object? _property;

    private Task _updateTask = Task.CompletedTask;

    protected UpdatableProperty()
    {
    }

    public object? Property
    {
        get => _property;
        protected set => SetProperty(ref _property, value);
    }

    public Task<object?> Get(bool force = false)
    {
        lock (_locker)
        {
            if (_getTask.IsCompleted)
            {
                _getTask = Task.Run(async () =>
                {
                    if (Property == default || force)
                        await Update().ConfigureAwait(false);

                    return Property;
                });
            }

            return _getTask;
        }
    }

    public Task Update()
    {
        lock (_locker)
        {
            if (_updateTask.IsCompleted) _updateTask = InnerUpdate();
            return _updateTask;
        }
    }

    protected abstract Task InnerUpdate();
}

public sealed class UpdatableProperty<PropertyType> : UpdatableProperty
{
    public UpdatableProperty(Func<Task<PropertyType>> updatableFunction)
    {
        UpdatableFunction = updatableFunction ?? throw new ArgumentNullException(nameof(updatableFunction), $"{nameof(updatableFunction)} must be set");
    }

    public new PropertyType? Property => (PropertyType?)base.Property;
    private Func<Task<PropertyType>> UpdatableFunction { get; }

    public new async Task<PropertyType?> Get(bool force = false)
    {
        return (PropertyType?)await base.Get(force).ConfigureAwait(false);
    }

    protected override async Task InnerUpdate()
    {
        base.Property = await UpdatableFunction.Invoke().ConfigureAwait(false);
    }
}