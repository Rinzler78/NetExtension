## MODIFIED Requirements

### Requirement: Thread-Safe Initialization Flag
The `_initialized` flag SHALL only be read and written within an async-safe mutual
exclusion scope (using `SemaphoreSlim(1, 1)`), preventing multiple concurrent callers
from each triggering an update on first access.

The internal `lock (_locker)` for the synchronous task-deduplication check in `Get()`
and `Update()` is replaced by `SemaphoreSlim.WaitAsync()` to allow `await` inside
the critical section.

#### Scenario: Concurrent first access — single update
- **WHEN** 10 threads call `Get()` simultaneously before initialization
- **THEN** `InnerUpdate` is called exactly once and all callers receive the same result

#### Scenario: Sequential access after init — no update
- **WHEN** `Get()` is called after initialization has completed
- **THEN** `InnerUpdate` is NOT called and the cached value is returned immediately

## MODIFIED Requirements

### Requirement: Single-Flight Task Deduplication
`Get()` and `Update()` SHALL use task deduplication: if a fetch/update task is already
in progress, the same `Task` is returned to all callers rather than starting a new one.
The deduplication check and the `_initialized` flag check SHALL be performed atomically
within the same `SemaphoreSlim` scope.

#### Scenario: Concurrent Get calls during update
- **WHEN** `Get()` is called concurrently while an update is in progress
- **THEN** all callers receive the same `Task` instance and `InnerUpdate` is not called twice
