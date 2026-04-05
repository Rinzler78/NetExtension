## ADDED Requirements

### Requirement: Single-Flight Returns Same Task Instance
`Invoke()` SHALL return the same `Task` instance when called while a previous invocation is running.
When `Invoke()` is called while a previous task is still running,
the exact same `Task` instance SHALL be returned (reference equality).

#### Scenario: Concurrent Invoke returns identical Task reference
- **WHEN** `Invoke()` is called a second time before the first task completes
- **THEN** the returned `Task` is reference-equal to the first

### Requirement: Dispose Idempotent
`Dispose()` SHALL be idempotent and SHALL NOT throw on multiple calls.
Calling `Dispose()` multiple times SHALL NOT throw.

#### Scenario: Double dispose does not throw
- **WHEN** `Dispose()` is called twice
- **THEN** no exception is thrown
