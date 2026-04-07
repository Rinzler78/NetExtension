# Capability: Reusable Task

## Purpose
`ReusableTask` wraps a synchronous `Action` and allows it to be invoked asynchronously,
with cancellation support and deduplication (only one invocation at a time).

## Requirements

### Requirement: Single-flight Invocation
`ReusableTask.Invoke()` SHALL return the in-progress task if a previous invocation
has not yet completed, avoiding duplicate concurrent executions of the wrapped action.

#### Scenario: Already running
- **WHEN** `Invoke()` is called while a previous task is still running
- **THEN** the same in-progress `Task` is returned without starting a new one

#### Scenario: Completed task
- **WHEN** `Invoke()` is called after a previous task has completed
- **THEN** a new `Task` is started and returned

### Requirement: Cancellation
`ReusableTask.Cancel()` SHALL signal the current `CancellationToken` to cancel
the in-progress action if it checks the token.
The method SHALL return `true` if a task was running and cancellation was requested,
`false` if no task has been started or if already cancelled.

#### Scenario: Cancel running task
- **WHEN** `Cancel()` is called while a task is running
- **THEN** the associated `CancellationTokenSource` is cancelled and `true` is returned

#### Scenario: Cancel with no task
- **WHEN** `Cancel()` is called when no task has been started
- **THEN** returns `false`

### Requirement: Resource Disposal
`ReusableTask` SHALL implement `IDisposable`. Calling `Dispose()` SHALL dispose the
current `CancellationTokenSource`. Each call to `Invoke()` that creates a new
`CancellationTokenSource` SHALL dispose the previous one before replacing it.

#### Scenario: Dispose releases resources
- **WHEN** `Dispose()` is called
- **THEN** the internal `CancellationTokenSource` is disposed without throwing

#### Scenario: Invoke disposes previous CTS
- **WHEN** `Invoke()` is called after a previous task has completed
- **THEN** the old `CancellationTokenSource` is disposed before a new one is created

### Requirement: Synchronous Invocation
`ReusableTask.InvokeSync()` SHALL execute the wrapped action synchronously on the
calling thread if cancellation has not been requested.

#### Scenario: Execute synchronously
- **WHEN** cancellation has not been requested
- **THEN** the action is executed synchronously within the lock

### Requirement: Single-Flight Returns Same Task
When `Invoke()` is called while a previous task is still running,
the exact same `Task` instance SHALL be returned.

#### Scenario: Concurrent Invoke returns identical Task
- **WHEN** `Invoke()` is called a second time before the first task completes
- **THEN** the returned `Task` is reference-equal to the first one

### Requirement: Dispose Idempotent
Calling `Dispose()` multiple times SHALL be safe and SHALL NOT throw.

#### Scenario: Double dispose does not throw
- **WHEN** `Dispose()` is called twice
- **THEN** no exception is thrown
