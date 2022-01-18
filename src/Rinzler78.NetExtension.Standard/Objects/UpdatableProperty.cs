using System;
using System.Linq;
using System.Threading.Tasks;
using Rinzler78.NetExtension.Observable;

namespace Rinzler78.NetExtension.Objects
{
    public static class UpdatablePropertyExtension
    {
        public static async System.Threading.Tasks.Task GetAll(this UpdatableProperty[] updatableProperties)
        {
            await System.Threading.Tasks.Task.WhenAll(updatableProperties.Select(arg => arg.Get())).ConfigureAwait(false);
        }

        public static async System.Threading.Tasks.Task UpdateAll(this UpdatableProperty[] updatableProperties)
        {
            await System.Threading.Tasks.Task.WhenAll(updatableProperties.Select(arg => arg.Update())).ConfigureAwait(false);
        }
    }

    public abstract class UpdatableProperty : ObservableObject
    {
        readonly object locker;

        object _property;
        public object Property
        {
            get => _property;
            protected set => SetProperty(ref _property, value);
        }

        public UpdatableProperty()
        {
            locker = new object();
            Property = default;
        }

        Task<object> _getTask;
        public Task<object> Get(bool force = false)
        {
            lock (locker)
            {
                if (_getTask?.IsCompleted ?? true)
                {
                    _getTask = System.Threading.Tasks.Task.Run(async () =>
                    {
                        if (Property == default || force)
                            await Update();

                        return Property;
                    });
                }
                return _getTask;
            }
        }

        System.Threading.Tasks.Task _updateTask;
        public System.Threading.Tasks.Task Update()
        {
            lock (locker)
            {
                if (_updateTask?.IsCompleted ?? true)
                {
                    _updateTask = InnerUpdate();
                }
                return _updateTask;
            }
        }

        protected abstract System.Threading.Tasks.Task InnerUpdate();
    }

    public class UpdatableProperty<PropertyType> : UpdatableProperty
    {
        public new PropertyType Property => (PropertyType)base.Property;
        Func<Task<PropertyType>> UpdatableFunction { get; }

        public UpdatableProperty(Func<Task<PropertyType>> updatableFunction) : base()
        {
            UpdatableFunction = updatableFunction;
        }

        public new async Task<PropertyType> Get(bool force = false)
        {
            return (PropertyType)await base.Get(force);
        }

        protected override async System.Threading.Tasks.Task InnerUpdate()
        {
            base.Property = await UpdatableFunction?.Invoke();
        }
    }
}
