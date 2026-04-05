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
