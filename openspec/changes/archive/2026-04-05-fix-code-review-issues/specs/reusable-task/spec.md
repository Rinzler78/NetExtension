## MODIFIED Requirements

### Requirement: Single-flight Invocation
`ReusableTask.Invoke()` SHALL return the in-progress task if a previous invocation
has not yet completed, avoiding duplicate concurrent executions of the wrapped action.
Before creating a new `CancellationTokenSource`, the previous one SHALL be disposed.

#### Scenario: Already running
- **WHEN** `Invoke()` is called while a previous task is still running
- **THEN** the same in-progress `Task` is returned without starting a new one

#### Scenario: Completed task with CTS rotation
- **WHEN** `Invoke()` is called after a previous task has completed
- **THEN** the old `CancellationTokenSource` is disposed, a new one is created,
  and a new `Task` is started and returned

#### Scenario: Invoke after Dispose throws
- **WHEN** `Invoke()` is called after `Dispose()` has been called
- **THEN** `ObjectDisposedException` is thrown

## MODIFIED Requirements

### Requirement: Cancellation
`ReusableTask.Cancel()` SHALL signal the current `CancellationToken` to cancel
the in-progress action if it checks the token.
The method SHALL return `true` if a task was running and cancellation was requested,
`false` if no task has been started or if already cancelled.

#### Scenario: Cancel running task
- **WHEN** `Cancel()` is called while a task is running
- **THEN** the associated `CancellationTokenSource` is cancelled and `true` is returned

#### Scenario: Cancel with no task started
- **WHEN** `Cancel()` is called before any `Invoke()` was called
- **THEN** returns `false`

#### Scenario: Cancel when already cancelled
- **WHEN** `Cancel()` is called twice in a row
- **THEN** the second call returns `false`

## ADDED Requirements

### Requirement: Resource Disposal
`ReusableTask` SHALL implement `IDisposable`. Calling `Dispose()` SHALL cancel and
dispose the current `CancellationTokenSource`. Subsequent calls to `Invoke()`,
`InvokeSync()`, or `Cancel()` after `Dispose()` SHALL throw `ObjectDisposedException`.

#### Scenario: Dispose releases CancellationTokenSource
- **WHEN** `Dispose()` is called
- **THEN** the internal `CancellationTokenSource` is disposed without throwing

#### Scenario: Operations after Dispose throw
- **WHEN** `Invoke()`, `InvokeSync()`, or `Cancel()` is called after `Dispose()`
- **THEN** `ObjectDisposedException` is thrown
