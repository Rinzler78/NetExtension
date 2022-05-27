using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Rinzler78.NetExtension.Observable;

public abstract class ObservableObject : IObservableObject, IDisposable
{
    private bool _disposedValue;

    public ObservableObject()
    {
        NickName = GetType().Name;
        Dependencies.CollectionChanged += DependenciesCollectionChanged;
    }

    public ObservableCollection<IObservableObject> Dependencies { get; } = new();

    void IDisposable.Dispose()
    {
        Console.WriteLine($"{NickName} : Dispose");
        Dispose(true);
        DetachDependencies();
        GC.SuppressFinalize(this);
    }

    public virtual string NickName { get; }

    public event PropertyChangedEventHandler PropertyChanged;

    protected bool SetProperty<T>(ref T target, T source, Action<(T OldValue, T NewValue)> propertyChanged = null,
        Func<(T OldValue, T NewValue), bool> checkValidity = null, [CallerMemberName] string propertyName = null)
    {
        checkValidity ??= arg => EqualityComparer<T>.Default.Equals(arg.OldValue, arg.NewValue);

        if (EqualityComparer<T>.Default.Equals(target, source))
            return false;

        var oldValue = target;

        target = source;

        if (oldValue != null && oldValue is IObservableObject oldObservableObject)
            DetachDependencies(oldObservableObject);

        if (target != null && target is IObservableObject newObservableObject)
            AttachDependencies(newObservableObject);

        propertyChanged?.Invoke((oldValue, target));
        OnPropertyChanged(propertyName, oldValue, target);

        return true;
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null, object oldValue = null,
        object newValue = null)
    {
        if (PropertyChanged != null)
        {
#if DEBUG
            Console.WriteLine($"{GetType().Name} ({NickName}) : {propertyName} Changed : {oldValue} => {newValue}");
#endif
            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing) PropertyChanged = null;

            _disposedValue = true;
        }
    }

    private void DependenciesCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
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
        Dispose(false);
    }

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

    protected virtual void OnDependenciesPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
    }
}