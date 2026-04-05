## ADDED Requirements

### Requirement: SetProperty Auto-Manages IObservableObject Dependencies
`SetProperty` SHALL automatically attach and detach `IObservableObject` dependencies.
When `SetProperty` assigns a value implementing `IObservableObject`, the old value
SHALL be automatically detached from `Dependencies` and the new value SHALL be
automatically attached.

#### Scenario: Assigning new IObservableObject attaches it
- **WHEN** a property of type `IObservableObject` is set via `SetProperty` for the first time
- **THEN** the new value appears in `Dependencies`

#### Scenario: Replacing IObservableObject detaches old and attaches new
- **WHEN** a property of type `IObservableObject` is replaced with a different instance
- **THEN** the old value is removed from `Dependencies` and the new value is added

#### Scenario: Setting to null detaches the previous value
- **WHEN** a property of type `IObservableObject` is set to null
- **THEN** the previous value is detached and `Dependencies` is empty

### Requirement: SetProperty PropertyChanged Callback
`SetProperty` SHALL invoke the `propertyChanged` callback when the value changes.
When a non-null `Action<(T OldValue, T NewValue)>` callback is provided to `SetProperty`,
it SHALL be invoked when the property changes and SHALL NOT be invoked when unchanged.

#### Scenario: Callback invoked with correct values on change
- **WHEN** `SetProperty` is called with a non-null `propertyChanged` callback and the value changes
- **THEN** the callback receives the previous value as `OldValue` and the new value as `NewValue`

#### Scenario: Callback not invoked when value unchanged
- **WHEN** `SetProperty` is called with the same value
- **THEN** the callback is NOT invoked

### Requirement: Dispose Idempotent
Calling `Dispose()` multiple times SHALL be safe and SHALL NOT throw.

#### Scenario: Double dispose does not throw
- **WHEN** `Dispose()` is called twice on the same instance
- **THEN** no exception is thrown

### Requirement: AttachDependencies Deduplication in Large Collections
When `Dependencies.Count >= 10`, `AttachDependencies` SHALL use a `HashSet` for
O(n) duplicate detection. Attempts to attach an already-present dependency SHALL
be silently ignored.

#### Scenario: Duplicate attach on large collection is filtered
- **WHEN** `AttachDependencies` is called with an already-present dependency in a 11+ collection
- **THEN** the dependency count does not increase
