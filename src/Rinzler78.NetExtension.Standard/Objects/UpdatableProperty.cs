using Rinzler78.NetExtension.Observable;
using System;
using System.Threading.Tasks;

namespace Rinzler78.NetExtension.Objects
{
    public abstract class UpdatableProperty : ObservableObject
    {
        private readonly object locker;

        private object _property;

        public object Property
        {
            get => _property;
            protected set => SetProperty(ref _property, value);
        }

        public UpdatableProperty()
        {
            locker = new object();
        }

        private Task<object> _getTask;

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

        private System.Threading.Tasks.Task _updateTask;

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
        private Func<Task<PropertyType>> UpdatableFunction { get; }

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