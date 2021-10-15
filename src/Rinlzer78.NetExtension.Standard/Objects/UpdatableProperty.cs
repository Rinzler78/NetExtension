using System;
using System.Threading.Tasks;
using Rinlzer78.NetExtension.Observable;

namespace Rinlzer78.NetExtension.Objects
{
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
            _updateTask = null;
        }

        Task<object> _GetTask;
        public Task<object> Get(bool force = false)
        {
            lock (locker)
            {
                if (_GetTask?.IsCompleted ?? true)
                {
                    _GetTask = Task.Run(async () =>
                    {
                        if (Property == default || force)
                            await Update();

                        return Property;
                    });
                }
                return _GetTask;
            }
        }

        Task _updateTask;
        public Task Update()
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

        protected abstract Task InnerUpdate();
    }

    public class UpdatableProperty<PropertyType> : UpdatableProperty
    {
        public new PropertyType Property => (PropertyType)base.Property;
        Func<Task<PropertyType>> UpdatableFunction { get; }

        public UpdatableProperty(Func<Task<PropertyType>> updatableFunction) : base()
        {
            UpdatableFunction = updatableFunction;
        }

        public new Task<PropertyType> Get(bool force = false) => base.Get(force) as Task<PropertyType>;
        protected override async Task InnerUpdate()
        {
            base.Property = await UpdatableFunction?.Invoke();
        }
    }
}
