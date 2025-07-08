# 🔔 Observable Pattern

[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?style=flat-square&logo=dotnet)](https://dotnet.microsoft.com/)
[![Thread Safe](https://img.shields.io/badge/Thread-Safe-green?style=flat-square)](https://docs.microsoft.com/en-us/dotnet/standard/threading/)

The Observable pattern implementation provides a robust foundation for reactive programming with automatic property change notifications and dependency management.

## 📋 Table of Contents

- [Overview](#overview)
- [Core Classes](#core-classes)
- [Key Features](#key-features)
- [Usage Examples](#usage-examples)
- [Advanced Scenarios](#advanced-scenarios)
- [Performance Considerations](#performance-considerations)
- [Best Practices](#best-practices)

## Overview

The Observable system in Rinzler78.NetExtension provides a complete implementation of the Observer pattern with advanced features like automatic dependency tracking, thread-safe operations, and memory-efficient disposal patterns.

## Core Classes

### `ObservableObject`
Base abstract class that implements `IObservableObject` and `IDisposable`.

```csharp
public abstract class ObservableObject : IObservableObject, IDisposable
{
    public virtual string? NickName { get; }
    public event PropertyChangedEventHandler? PropertyChanged;
    public ObservableCollection<IObservableObject> Dependencies { get; }
    
    protected bool SetProperty<T>(ref T target, T source, 
        Action<(T OldValue, T NewValue)>? propertyChanged = null,
        Func<(T OldValue, T NewValue), bool>? checkValidity = null,
        [CallerMemberName] string? propertyName = null)
}
```

### `IObservableObject`
Interface extending `INotifyPropertyChanged` with a `NickName` property for debugging.

```csharp
public interface IObservableObject : INotifyPropertyChanged
{
    string? NickName { get; }
}
```

### `ObservableRangeCollection<T>`
Enhanced observable collection supporting range operations.

```csharp
public class ObservableRangeCollection<T> : ObservableCollection<T>
{
    public void AddRange(IEnumerable<T> items)
    public void RemoveRange(IEnumerable<T> items)
    public void ReplaceRange(IEnumerable<T> items)
}
```

### `ObservablePropertyAttribute`
Attribute for marking properties as observable (used with source generators).

```csharp
[AttributeUsage(AttributeTargets.Property)]
public class ObservablePropertyAttribute : Attribute
{
}
```

## Key Features

### 🔐 Thread-Safe Operations
All property changes are protected by locks to ensure thread safety:

```csharp
protected bool SetProperty<T>(ref T target, T source, ...)
{
    lock (Dependencies)
    {
        // Thread-safe property setting
        if (checkValidity.Invoke((target, source)))
            return false;
            
        var oldValue = target;
        target = source;
        
        // Automatic dependency management
        if (oldValue is IObservableObject oldObservable)
            DetachDependencies(oldObservable);
            
        if (target is IObservableObject newObservable)
            AttachDependencies(newObservable);
            
        propertyChanged?.Invoke((oldValue, target));
        OnPropertyChanged(propertyName, oldValue, target);
        
        return true;
    }
}
```

### 🔗 Automatic Dependency Tracking
Objects automatically track dependencies and propagate property changes:

```csharp
public class ParentModel : ObservableObject
{
    private ChildModel _child;
    
    public ChildModel Child
    {
        get => _child;
        set => SetProperty(ref _child, value); // Automatically manages dependencies
    }
}
```

### 🧹 Memory-Efficient Disposal
Proper disposal pattern with automatic cleanup:

```csharp
protected virtual void Dispose(bool disposing)
{
    if (!_disposedValue)
    {
        if (disposing) 
        {
            PropertyChanged = null;
            DetachDependencies(); // Clean up all dependencies
        }
        _disposedValue = true;
    }
}
```

### 🐛 Debug Support
Conditional compilation for debugging property changes:

```csharp
#if TRACE_PROPERTY_CHANGED
    Console.WriteLine($"{GetType().Name} ({NickName}) : {propertyName} Changed : {oldValue} => {newValue}");
#endif
```

## Usage Examples

### Basic Observable Model

```csharp
public class UserModel : ObservableObject
{
    private string _name;
    private string _email;
    private bool _isActive;
    
    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }
    
    public string Email
    {
        get => _email;
        set => SetProperty(ref _email, value, 
            propertyChanged: (change) => ValidateEmail(change.NewValue));
    }
    
    public bool IsActive
    {
        get => _isActive;
        set => SetProperty(ref _isActive, value);
    }
    
    private void ValidateEmail(string email)
    {
        // Custom validation logic
        if (!email.IsValidEmail())
            throw new ArgumentException("Invalid email format");
    }
}
```

### Property Change Callbacks

```csharp
public class CounterModel : ObservableObject
{
    private int _count;
    
    public int Count
    {
        get => _count;
        set => SetProperty(ref _count, value,
            propertyChanged: (change) => 
            {
                Console.WriteLine($"Count changed from {change.OldValue} to {change.NewValue}");
                OnCountChanged(change.OldValue, change.NewValue);
            });
    }
    
    private void OnCountChanged(int oldValue, int newValue)
    {
        // Custom logic when count changes
        if (newValue > 100)
            throw new InvalidOperationException("Count cannot exceed 100");
    }
}
```

### Custom Validity Checking

```csharp
public class RangeModel : ObservableObject
{
    private int _value;
    
    public int Value
    {
        get => _value;
        set => SetProperty(ref _value, value,
            checkValidity: (change) => 
            {
                // Custom equality check
                return change.OldValue == change.NewValue || 
                       (change.NewValue < 0 || change.NewValue > 100);
            });
    }
}
```

### Observable Collections

```csharp
public class PlaylistModel : ObservableObject
{
    public ObservableRangeCollection<Song> Songs { get; }
    
    public PlaylistModel()
    {
        Songs = new ObservableRangeCollection<Song>();
        Songs.CollectionChanged += OnSongsChanged;
    }
    
    public void AddSongs(IEnumerable<Song> songs)
    {
        Songs.AddRange(songs); // Efficient bulk operation
    }
    
    private void OnSongsChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        OnPropertyChanged(nameof(SongCount));
    }
    
    public int SongCount => Songs.Count;
}
```

## Advanced Scenarios

### Dependency Chains

```csharp
public class OrderModel : ObservableObject
{
    private CustomerModel _customer;
    private ObservableRangeCollection<OrderItem> _items;
    
    public CustomerModel Customer
    {
        get => _customer;
        set => SetProperty(ref _customer, value);
    }
    
    public ObservableRangeCollection<OrderItem> Items
    {
        get => _items;
        set => SetProperty(ref _items, value);
    }
    
    public decimal TotalAmount => Items?.Sum(item => item.Total) ?? 0;
    
    protected override void OnDependenciesPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        base.OnDependenciesPropertyChanged(sender, e);
        
        // Recalculate total when items change
        if (sender is OrderItem && (e.PropertyName == nameof(OrderItem.Quantity) || 
                                   e.PropertyName == nameof(OrderItem.Price)))
        {
            OnPropertyChanged(nameof(TotalAmount));
        }
    }
}
```

### Manual Dependency Management

```csharp
public class CompositeModel : ObservableObject
{
    private readonly List<IObservableObject> _manualDependencies = new();
    
    public void AddDependency(IObservableObject dependency)
    {
        _manualDependencies.Add(dependency);
        AttachDependencies(dependency);
    }
    
    public void RemoveDependency(IObservableObject dependency)
    {
        _manualDependencies.Remove(dependency);
        DetachDependencies(dependency);
    }
    
    protected override void OnDependenciesPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        // Handle changes from manual dependencies
        if (_manualDependencies.Contains(sender))
        {
            OnPropertyChanged("ComputedValue");
        }
    }
}
```

## Performance Considerations

### Memory Management
- Use `using` statements or explicit disposal for observable objects
- Avoid circular references in dependency chains
- Consider weak references for large dependency graphs

### Threading
- All property changes are thread-safe through internal locking
- Property change events are raised on the calling thread
- Consider using `SynchronizationContext` for UI updates

### Performance Tips
```csharp
// Batch property changes
public void UpdateUserInfo(string name, string email, bool isActive)
{
    // Disable notifications temporarily if needed
    _name = name;
    _email = email;
    _isActive = isActive;
    
    // Single notification
    OnPropertyChanged(nameof(Name));
    OnPropertyChanged(nameof(Email));
    OnPropertyChanged(nameof(IsActive));
}
```

## Best Practices

### 1. Naming Conventions
```csharp
public class MyModel : ObservableObject
{
    public MyModel()
    {
        NickName = "MyModel"; // Useful for debugging
    }
}
```

### 2. Property Validation
```csharp
public string Name
{
    get => _name;
    set => SetProperty(ref _name, value,
        checkValidity: (change) => !string.IsNullOrWhiteSpace(change.NewValue));
}
```

### 3. Dependency Cleanup
```csharp
protected override void Dispose(bool disposing)
{
    if (disposing)
    {
        // Clean up manual subscriptions
        ExternalService.PropertyChanged -= OnExternalServiceChanged;
    }
    base.Dispose(disposing);
}
```

### 4. Debugging Support
Enable debug traces by defining compilation symbols:
```csharp
#define TRACE_PROPERTY_CHANGED
#define TRACE_PROPERTY_ATTACH_DETACH
#define TRACE_DISPOSE
```

---

[← Back to Main Documentation](../README.md)