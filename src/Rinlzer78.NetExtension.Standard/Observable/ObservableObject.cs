using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Rinlzer78.NetExtension.Observable
{
    public abstract class ObservableObject : IObservableObject, IDisposable
    {
        private bool disposedValue;

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

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                    PropertyChanged = null;
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        ~ObservableObject()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: false);
        }

        void IDisposable.Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
