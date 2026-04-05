# Capability: Updatable Property

## Purpose
`UpdatableProperty<T>` provides a lazily-initialized, asynchronously-fetched observable
property. It fetches its value on first access and supports forced refresh.
It inherits from `ObservableObject` for MVVM integration.

## Requirements

### Requirement: Lazy Initialization
`UpdatableProperty<T>.Get(force: false)` SHALL trigger an `InnerUpdate` call the first
time it is invoked, then cache the result for subsequent calls unless `force` is `true`.

#### Scenario: First access triggers update
- **WHEN** `Get()` is called for the first time
- **THEN** `InnerUpdate` is called and the result is stored in `Property`

#### Scenario: Subsequent access uses cache
- **WHEN** `Get()` is called after a successful first access
- **THEN** `InnerUpdate` is NOT called again and the cached `Property` value is returned

#### Scenario: Forced refresh bypasses cache
- **WHEN** `Get(force: true)` is called
- **THEN** `InnerUpdate` is called regardless of the cached state

### Requirement: Thread-Safe Initialization Flag
The `_initialized` flag SHALL only be read and written under mutual exclusion,
preventing multiple concurrent callers from each triggering an update on first access.

#### Scenario: Concurrent first access
- **WHEN** multiple threads call `Get()` simultaneously before initialization
- **THEN** `InnerUpdate` is called exactly once (single-flight deduplication)

### Requirement: Single-Flight Task Deduplication
`Get()` and `Update()` SHALL use task deduplication: if a fetch/update task is already
in progress, the same `Task` is returned to all callers rather than starting a new one.

#### Scenario: Concurrent Get calls
- **WHEN** `Get()` is called concurrently while an update is in progress
- **THEN** all callers receive the same `Task` instance
