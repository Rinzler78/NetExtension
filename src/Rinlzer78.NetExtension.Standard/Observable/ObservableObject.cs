using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Rinlzer78.NetExtension
{
    public abstract class ObservableObject : IObservableObject
    {
        protected bool SetProperty<T>(ref T target, T source, Action<(T OldValue, T NewValue)> propertyChanged = null, Func<(T OldValue, T NewValue), bool> CheckValidity = null, [CallerMemberName] string propertyName = null)
        {
            if(CheckValidity == null)
                CheckValidity = (arg) => EqualityComparer<T>.Default.Equals(arg.OldValue, arg.NewValue);

            if (EqualityComparer<T>.Default.Equals(target, source))
                return false;
#if DEBUG
            Console.WriteLine($"{GetType().Name} : {propertyName} Changed : {target} => {source}");
#endif
            T oldValue = target;

            target = source;

            propertyChanged?.Invoke((oldValue, target));
            OnPropertyChanged(propertyName);

            return true;
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
