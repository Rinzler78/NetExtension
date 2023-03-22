#if DEBUG
//#define TRACE_PROPERTY_CHANGED
//#define TRACE_PROPERTY_ATTACH_DETACH
//#define TRACE_DISPOSE
//#define TRACE_CTOR
#endif

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;

namespace Rinzler78.NetExtension.Observable;

public abstract class ObservableObject : IObservableObject, IDisposable
{
    private bool _disposedValue;

    protected ObservableObject(params IObservableObject[] dependentManagers)
    {
        NickName = GetType().Name;
        Dependencies.CollectionChanged += DependenciesCollectionChanged;

        AttachDependencies(dependentManagers);

#if TRACE_CTOR
        Console.WriteLine($"{NickName} : Ctor");
#endif
    }

    [JsonIgnore]
    public ObservableCollection<IObservableObject> Dependencies { get; } = new();

    void IDisposable.Dispose()
    {
#if TRACE_DISPOSE
        Console.WriteLine($"{NickName} : Dispose");
#endif
        Dispose(true);
        DetachDependencies();
        GC.SuppressFinalize(this);
    }

    [JsonIgnore]
    public virtual string NickName { get; }

    public event PropertyChangedEventHandler PropertyChanged;

    protected bool SetProperty<T>(ref T target, T source, Action<(T OldValue, T NewValue)> propertyChanged = null,
        Func<(T OldValue, T NewValue), bool> checkValidity = null, [CallerMemberName] string propertyName = null)
    {
        lock (Dependencies)
        {
            checkValidity ??= arg => EqualityComparer<T>.Default.Equals(arg.OldValue, arg.NewValue);

            if (checkValidity.Invoke((target, source)))
            {
                //if (EqualityComparer<T>.Default.Equals(target, source))
                return false;
            }

            var oldValue = target;

            target = source;

            if (oldValue is not null && oldValue is IObservableObject oldObservableObject)
                DetachDependencies(oldObservableObject);

            if (target is not null && target is IObservableObject newObservableObject)
                AttachDependencies(newObservableObject);

            propertyChanged?.Invoke((oldValue, target));
            OnPropertyChanged(propertyName, oldValue, target);

            return true;
        }
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null, object oldValue = null,
        object newValue = null)
    {
        if (PropertyChanged is not null)
        {
#if TRACE_PROPERTY_CHANGED
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
        if (e.NewItems is not null)
        {
            foreach (var item in e.NewItems)
                (item as IObservableObject).PropertyChanged += OnDependenciesPropertyChanged;
        }

        if (e.OldItems is not null)
        {
            foreach (var item in e.OldItems)
                (item as IObservableObject).PropertyChanged -= OnDependenciesPropertyChanged;
        }
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
#if TRACE_PROPERTY_ATTACH_DETACH
                Console.WriteLine($"{GetType().Name} ({NickName}) : Attach {dep.NickName}");
#endif
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
#if TRACE_PROPERTY_ATTACH_DETACH
                Console.WriteLine($"{GetType().Name} ({NickName}) : Detach {dep.NickName}");
#endif
                Dependencies.Remove(dep);
            }
        }
    }

    protected virtual void OnDependenciesPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
    }
}