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

/// <summary>
/// Foundation class for objects that implement property change notification and dependency management.
/// Provides MVVM-compatible property change notifications and automatic dependency tracking.
/// </summary>
public abstract class ObservableObject : IObservableObject, IDisposable
{
    private bool _disposedValue;

    /// <summary>
    /// Initializes a new instance of the ObservableObject class with optional dependent objects.
    /// The NickName is automatically set to the type name for debugging purposes.
    /// </summary>
    /// <param name="dependentManagers">Optional array of dependent observable objects to automatically track</param>
    protected ObservableObject(params IObservableObject[] dependentManagers)
    {
        NickName = GetType().Name;
        Dependencies.CollectionChanged += DependenciesCollectionChanged;

        AttachDependencies(dependentManagers);

#if TRACE_CTOR
        Console.WriteLine($"{NickName} : Ctor");
#endif
    }

    /// <summary>
    /// Gets the collection of dependent observable objects that this object monitors for property changes.
    /// When any dependent object raises PropertyChanged, this object can respond accordingly.
    /// This property is excluded from JSON serialization.
    /// </summary>
    [JsonIgnore]
    public ObservableCollection<IObservableObject> Dependencies { get; } = new();

    /// <summary>
    /// Releases all resources used by the ObservableObject and detaches all dependencies.
    /// This method is called automatically when the object is disposed.
    /// </summary>
    void IDisposable.Dispose()
    {
#if TRACE_DISPOSE
        Console.WriteLine($"{NickName} : Dispose");
#endif
        Dispose(true);
        DetachDependencies();
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Gets a friendly name for this object instance, typically used for debugging and logging.
    /// Defaults to the type name but can be overridden in derived classes.
    /// This property is excluded from JSON serialization.
    /// </summary>
    [JsonIgnore]
    public virtual string? NickName { get; }

    /// <summary>
    /// Occurs when a property value changes. This event is part of the INotifyPropertyChanged contract
    /// and enables two-way data binding in MVVM scenarios.
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Sets a property value and raises PropertyChanged if the value actually changes.
    /// Automatically manages dependencies for IObservableObject properties and provides thread safety.
    /// </summary>
    /// <typeparam name="T">The type of the property being set</typeparam>
    /// <param name="target">Reference to the backing field for the property</param>
    /// <param name="source">The new value to set</param>
    /// <param name="propertyChanged">Optional callback invoked when the property changes, receiving old and new values</param>
    /// <param name="checkValidity">Optional function to determine if values are equal (defaults to EqualityComparer)</param>
    /// <param name="propertyName">The name of the property (automatically inferred from caller)</param>
    /// <returns>True if the property value was changed, false if the new value equals the old value</returns>
    /// <remarks>
    /// If the property implements IObservableObject, it will be automatically attached/detached from the Dependencies collection.
    /// This method is thread-safe and uses locking to prevent race conditions.
    /// </remarks>
    /// <example>
    /// <code>
    /// private string _name;
    /// public string Name
    /// {
    ///     get => _name;
    ///     set => SetProperty(ref _name, value);
    /// }
    /// </code>
    /// </example>
    protected bool SetProperty<T>(ref T target, T source, Action<(T OldValue, T NewValue)>? propertyChanged = null,
        Func<(T OldValue, T NewValue), bool>? checkValidity = null, [CallerMemberName] string? propertyName = null)
    {
        lock (_locker)
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
                DetachDependenciesCore(new[] { oldObservableObject });

            if (target is not null && target is IObservableObject newObservableObject)
                AttachDependenciesCore(new[] { newObservableObject });

            propertyChanged?.Invoke((oldValue, target));
            OnPropertyChanged(propertyName, oldValue, target);

            return true;
        }
    }

    /// <summary>
    /// Raises the PropertyChanged event for the specified property.
    /// This method can be overridden in derived classes to add custom behavior when properties change.
    /// </summary>
    /// <param name="propertyName">The name of the property that changed (automatically inferred from caller)</param>
    /// <param name="oldValue">The previous value of the property (used for debugging when TRACE_PROPERTY_CHANGED is defined)</param>
    /// <param name="newValue">The new value of the property (used for debugging when TRACE_PROPERTY_CHANGED is defined)</param>
    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null, object? oldValue = null,
        object? newValue = null)
    {
        if (PropertyChanged is not null)
        {
#if TRACE_PROPERTY_CHANGED
            Console.WriteLine($"{GetType().Name} ({NickName}) : {propertyName} Changed : {oldValue} => {newValue}");
#endif
            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    /// <summary>
    /// Releases the unmanaged resources used by the ObservableObject and optionally releases the managed resources.
    /// </summary>
    /// <param name="disposing">True to release both managed and unmanaged resources; false to release only unmanaged resources</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing) PropertyChanged = null;

            _disposedValue = true;
        }
    }

    private void DependenciesCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.NewItems is not null)
        {
            foreach (var item in e.NewItems.Cast<IObservableObject>())
                item.PropertyChanged += OnDependenciesPropertyChanged;
        }

        if (e.OldItems is not null)
        {
            foreach (var item in e.OldItems.Cast<IObservableObject>())
                item.PropertyChanged -= OnDependenciesPropertyChanged;
        }
    }

    //~ObservableObject()
    //{
    //    Dispose(false);
    //}

    /// <summary>
    /// Attaches one or more observable objects as dependencies, causing this object to monitor their PropertyChanged events.
    /// Duplicate dependencies are automatically filtered out.
    /// </summary>
    /// <param name="observableObject">The observable objects to attach as dependencies</param>
    /// <remarks>
    /// <para>Performance characteristics:</para>
    /// <list type="bullet">
    /// <item>Small dependency collections (&lt; 10 items): O(n²) using direct Contains operations</item>
    /// <item>Large dependency collections (≥ 10 items): O(n) using HashSet for duplicate detection</item>
    /// </list>
    /// <para>This method is thread-safe and will not add duplicate dependencies.</para>
    /// <para>When dependencies are attached, their PropertyChanged events will trigger OnDependenciesPropertyChanged.</para>
    /// </remarks>
    protected void AttachDependencies(params IObservableObject[] observableObject)
    {
        lock (_locker)
            AttachDependenciesCore(observableObject);
    }

    private void AttachDependenciesCore(IObservableObject[] observableObject)
    {
        IEnumerable<IObservableObject> toAttach;

        // Use HashSet optimization for larger collections
        if (Dependencies.Count >= 10)
        {
            // O(n) performance using HashSet for lookups
            var dependenciesSet = new HashSet<IObservableObject>(Dependencies);
            toAttach = observableObject.Where(arg => !dependenciesSet.Contains(arg));
        }
        else
        {
            // O(n²) performance for small collections - more memory efficient
            toAttach = observableObject.Where(arg => !Dependencies.Contains(arg));
        }

        foreach (var dep in toAttach)
        {
#if TRACE_PROPERTY_ATTACH_DETACH
            Console.WriteLine($"{GetType().Name} ({NickName}) : Attach {dep.NickName}");
#endif
            Dependencies.Add(dep);
        }
    }

    private readonly object _locker = new();

    /// <summary>
    /// Detaches one or more observable objects from the dependencies collection, stopping monitoring of their PropertyChanged events.
    /// If no objects are specified, all current dependencies are detached.
    /// </summary>
    /// <param name="observableObject">The observable objects to detach. If null or empty, all dependencies are detached</param>
    /// <remarks>
    /// <para>Performance characteristics:</para>
    /// <list type="bullet">
    /// <item>Small dependency collections (&lt; 10 items): O(n²) using direct Contains operations</item>
    /// <item>Large dependency collections (≥ 10 items): O(n) using HashSet for existence checking</item>
    /// </list>
    /// <para>This method is thread-safe and will only detach objects that are currently in the Dependencies collection.</para>
    /// <para>Detached dependencies will no longer trigger OnDependenciesPropertyChanged when their properties change.</para>
    /// </remarks>
    protected void DetachDependencies(params IObservableObject[] observableObject)
    {
        lock (_locker)
            DetachDependenciesCore(observableObject);
    }

    private void DetachDependenciesCore(IObservableObject[] observableObject)
    {
        IEnumerable<IObservableObject> toDetach;

        if (observableObject == null || observableObject.Length == 0)
        {
            // Detach all dependencies
            toDetach = Dependencies.ToList(); // Create a copy to avoid collection modification during iteration
        }
        else
        {
            // Use HashSet optimization for larger collections
            if (Dependencies.Count >= 10)
            {
                // O(n) performance using HashSet for lookups
                var dependenciesSet = new HashSet<IObservableObject>(Dependencies);
                toDetach = observableObject.Where(dependenciesSet.Contains);
            }
            else
            {
                // O(n²) performance for small collections - more memory efficient
                toDetach = observableObject.Where(Dependencies.Contains);
            }
        }

        foreach (var dep in toDetach)
        {
#if TRACE_PROPERTY_ATTACH_DETACH
            Console.WriteLine($"{GetType().Name} ({NickName}) : Detach {dep.NickName}");
#endif
            Dependencies.Remove(dep);
        }
    }

    /// <summary>
    /// Called when any dependency object raises a PropertyChanged event.
    /// Override this method in derived classes to respond to changes in dependent objects.
    /// </summary>
    /// <param name="sender">The dependency object that raised the PropertyChanged event</param>
    /// <param name="e">The PropertyChangedEventArgs containing the name of the property that changed</param>
    /// <remarks>
    /// This method provides a centralized way to respond to changes in all dependency objects.
    /// The default implementation does nothing, so derived classes should override it to add custom behavior.
    /// </remarks>
    protected virtual void OnDependenciesPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
    }
}
