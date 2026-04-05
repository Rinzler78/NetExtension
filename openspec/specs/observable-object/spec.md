# Capability: Observable Object

## Purpose
`ObservableObject` is the MVVM base class implementing `INotifyPropertyChanged`.
It manages property change notifications, automatic dependency tracking between
observable objects, and thread-safe property mutation via `SetProperty<T>`.

## Requirements

### Requirement: Thread-Safe Property Set
`SetProperty<T>(ref T target, T source, ...)` SHALL update the backing field and
raise `PropertyChanged` only when the new value differs from the old one.
All mutations SHALL be performed under the internal `_locker` lock.

#### Scenario: Value changes
- **WHEN** `SetProperty` is called with a value different from the current one
- **THEN** the field is updated, `PropertyChanged` is raised, and `true` is returned

#### Scenario: Same value
- **WHEN** `SetProperty` is called with the same value
- **THEN** nothing changes, `PropertyChanged` is NOT raised, and `false` is returned

### Requirement: Automatic Dependency Management
When a property of type `IObservableObject` is set via `SetProperty`, the old value
SHALL be detached from `Dependencies` and the new value SHALL be attached automatically.

#### Scenario: Replace observable dependency
- **WHEN** an `IObservableObject` property is set to a new value via `SetProperty`
- **THEN** the old object is removed from `Dependencies` and the new one is added

### Requirement: Dependency Attach / Detach
`AttachDependencies` and `DetachDependencies` SHALL add or remove objects from the
`Dependencies` collection without adding duplicates. Both methods SHALL be thread-safe.

#### Scenario: No duplicate dependencies
- **WHEN** the same `IObservableObject` is attached twice
- **THEN** it appears only once in `Dependencies`

### Requirement: Dispose Detaches All Dependencies
Calling `Dispose()` on an `ObservableObject` SHALL detach all entries from `Dependencies`
and set `PropertyChanged` to `null`, preventing memory leaks.

#### Scenario: Dispose clears subscriptions
- **WHEN** `Dispose()` is called
- **THEN** `Dependencies` is empty and `PropertyChanged` is null

### Requirement: SetProperty Auto-Manages IObservableObject Dependencies
When `SetProperty` assigns a value that implements `IObservableObject`, the old
value SHALL be automatically detached from `Dependencies` and the new value SHALL
be automatically attached, without requiring manual calls to `AttachDependencies`.

#### Scenario: Assigning new IObservableObject attaches it as dependency
- **WHEN** a property of type `IObservableObject` is set via `SetProperty` for the first time
- **THEN** the new value appears in `Dependencies`

#### Scenario: Replacing IObservableObject detaches old and attaches new
- **WHEN** a property of type `IObservableObject` is replaced with a different instance
- **THEN** the old value is removed from `Dependencies` and the new value is added

#### Scenario: Setting to null detaches the previous IObservableObject
- **WHEN** a property of type `IObservableObject` is set to null
- **THEN** the previous value is detached and `Dependencies` no longer contains it

### Requirement: SetProperty PropertyChanged Callback
When a non-null `Action<(T OldValue, T NewValue)>` callback is provided to `SetProperty`,
it SHALL be invoked with the old and new values when the property changes.

#### Scenario: Callback invoked with correct values
- **WHEN** `SetProperty` is called with a non-null `propertyChanged` callback and the value changes
- **THEN** the callback receives the previous value as `OldValue` and the new value as `NewValue`

### Requirement: Dispose Idempotent
Calling `Dispose()` multiple times on an `ObservableObject` SHALL be safe
and SHALL NOT throw an exception on subsequent calls.

#### Scenario: Double dispose does not throw
- **WHEN** `Dispose()` is called twice on the same instance
- **THEN** no exception is thrown and the object remains in a valid disposed state

### Requirement: AttachDependencies Deduplication in Large Collections
When `Dependencies.Count >= 10`, `AttachDependencies` SHALL use a `HashSet` for
O(n) duplicate detection. Attempts to attach an already-present dependency SHALL
be silently ignored.

#### Scenario: Duplicate attach on large collection is filtered
- **WHEN** `AttachDependencies` is called with a dependency already present in a collection of 11+
- **THEN** the dependency count does not increase
