using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Rinzler78.NetExtension.Observable
{
    public abstract class ObservableObject : IObservableObject, IDisposable
    {
        private bool disposedValue;

        protected bool SetProperty<T>(ref T target, T source, Action<(T OldValue, T NewValue)> propertyChanged = null, Func<(T OldValue, T NewValue), bool> CheckValidity = null, [CallerMemberName] string propertyName = null)
        {
            CheckValidity ??= ((arg) => EqualityComparer<T>.Default.Equals(arg.OldValue, arg.NewValue));

            if (EqualityComparer<T>.Default.Equals(target, source))
                return false;

            T oldValue = target;

            target = source;

            if (oldValue != null && oldValue is IObservableObject oldObservableObject)
                DetachDependencies(oldObservableObject);

            if (target != null && target is IObservableObject newObservableObject)
                AttachDependencies(newObservableObject);

            propertyChanged?.Invoke((oldValue, target));
            OnPropertyChanged(propertyName, oldValue, target);

            return true;
        }

        public virtual string NickName { get; }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null, object oldValue = null, object newValue = null)
        {
            if (PropertyChanged != null)
            {
#if DEBUG
                Console.WriteLine($"{GetType().Name} ({NickName}) : {propertyName} Changed : {oldValue} => {newValue}");
#endif
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    PropertyChanged = null;
                }

                disposedValue = true;
            }
        }

        public ObservableObject()
        {
            NickName = GetType().Name;
            Dependencies.CollectionChanged += DependenciesCollectionChanged;
        }

        private void DependenciesCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
                foreach (var item in e.NewItems)
                    (item as IObservableObject).PropertyChanged += OnDependenciesPropertyChanged;

            if (e.OldItems != null)
                foreach (var item in e.OldItems)
                    (item as IObservableObject).PropertyChanged -= OnDependenciesPropertyChanged;
        }

        ~ObservableObject()
        {
            Dispose(disposing: false);
        }

        void IDisposable.Dispose()
        {
            Console.WriteLine($"{NickName} : Dispose");
            Dispose(disposing: true);
            DetachDependencies();
            GC.SuppressFinalize(this);
        }

        public ObservableCollection<IObservableObject> Dependencies { get; } = new ObservableCollection<IObservableObject>();

        protected void AttachDependencies(params IObservableObject[] observableObject)
        {
            lock (this)
            {
                var toAttach = observableObject.Where(arg => !Dependencies.Contains(arg));

                foreach (var dep in toAttach)
                {
                    Console.WriteLine($"{GetType().Name} ({NickName}) : Attach {dep.NickName}");
                    Dependencies.Add(dep);
                }
            }
        }

        protected void DetachDependencies(params IObservableObject[] observableObject)
        {
            lock (this)
            {
                var toAttach = observableObject?.Where(arg => Dependencies.Contains(arg)) ?? Dependencies;

                foreach (var dep in toAttach)
                {
                    Console.WriteLine($"{GetType().Name} ({NickName}) : Detach {dep.NickName}");
                    Dependencies.Remove(dep);
                }
            }
        }

        protected virtual void OnDependenciesPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
        }
    }
}